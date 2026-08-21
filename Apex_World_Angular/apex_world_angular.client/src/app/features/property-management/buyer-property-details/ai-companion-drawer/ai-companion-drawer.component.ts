import { Component, Input, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../../../environments/environment';

interface ChatMessage {
  sender: 'user' | 'ai';
  text: string;
  imagePreviewUrl?: string;
  isBookingWidget?: boolean;
  slots?: string[];
}

@Component({
  selector: 'app-ai-companion-drawer',
  templateUrl: './ai-companion-drawer.component.html',
  styleUrls: ['./ai-companion-drawer.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule]
})
export class AiCompanionDrawerComponent {
  @Input() propertyId: number | null = null;
  @Input() isOpen = false;
  @Output() closeDrawer = new EventEmitter<void>();

  chatHistory: ChatMessage[] = [];
  userInput = '';
  selectedImageBase64: string | null = null;
  selectedImagePreview: string | null = null;
  isLoading = false;

  constructor(private http: HttpClient) {}

  onClose() {
    this.closeDrawer.emit();
  }

  onImageSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = () => {
        this.selectedImagePreview = reader.result as string;
        this.selectedImageBase64 = (reader.result as string).split(',')[1];
      };
      reader.readAsDataURL(file);
    }
  }

  sendMessage() {
    if (!this.userInput.trim() && !this.selectedImageBase64) return;

    const userText = this.userInput;
    const imgPreview = this.selectedImagePreview;
    const imgBase64 = this.selectedImageBase64;

    this.chatHistory.push({
      sender: 'user',
      text: userText,
      imagePreviewUrl: imgPreview || undefined
    });

    this.userInput = '';
    this.selectedImagePreview = null;
    this.selectedImageBase64 = null;
    this.isLoading = true;

    // Send history to backend to preserve conversation context
    const historyPayload = this.chatHistory.slice(0, -1).map(msg => ({
      sender: msg.sender,
      text: msg.text
    }));

    const payload = {
      propertyId: this.propertyId,
      message: userText,
      imageBase64: imgBase64,
      chatHistory: historyPayload
    };

    this.http.post<any>(`${environment.apiUrl}/AiCompanion/chat`, payload).subscribe({
      next: (res) => {
        const reply = res.data?.replyText || "I couldn't get a response.";
        
        let isBookingWidget = false;
        let slots: string[] = [];
        
        // Match response to trigger scheduling widget UX
        const lowerReply = reply.toLowerCase();
        if (lowerReply.includes('success:') && lowerReply.includes('scheduled')) {
          // Success booking
        } else if (lowerReply.includes('already scheduled') || lowerReply.includes('already booked') || lowerReply.includes('already have a')) {
          // Already has booking, do not show widget
        } else if (lowerReply.includes('available') || lowerReply.includes('slots') || lowerReply.includes('schedule')) {
          isBookingWidget = true;
          slots = this.generateUpcomingSlots();
        }

        this.chatHistory.push({
          sender: 'ai',
          text: reply,
          isBookingWidget,
          slots
        });
      },
      error: (err) => {
        this.chatHistory.push({
          sender: 'ai',
          text: "Sorry, I had trouble connecting to the server. Please verify the Gemini API key is configured correctly."
        });
        console.error(err);
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }

  generateUpcomingSlots(): string[] {
    const slots = [];
    const today = new Date();
    for (let i = 1; i <= 3; i++) {
      const nextDay = new Date(today);
      nextDay.setDate(today.getDate() + i);
      const yyyymmdd = nextDay.toISOString().split('T')[0];
      slots.push(`${yyyymmdd} 10:00 AM`);
      slots.push(`${yyyymmdd} 02:00 PM`);
    }
    return slots;
  }

  bookSlot(slot: string) {
    this.isLoading = true;
    
    // Add user message on behalf of user to complete booking
    this.chatHistory.push({
      sender: 'user',
      text: `Book site visit for ${slot}`
    });

    const historyPayload = this.chatHistory.slice(0, -1).map(msg => ({
      sender: msg.sender,
      text: msg.text
    }));

    const payload = {
      propertyId: this.propertyId,
      message: `Please book the site visit for ${slot}. Details: First Name: Buyer, Last Name: User, Email: buyer@apexworld.com, Phone: +919999999999`,
      chatHistory: historyPayload
    };

    this.http.post<any>(`${environment.apiUrl}/AiCompanion/chat`, payload).subscribe({
      next: (res) => {
        this.chatHistory.push({
          sender: 'ai',
          text: res.data?.replyText || "Booking request processed."
        });
      },
      error: (err) => {
        this.chatHistory.push({
          sender: 'ai',
          text: "Error booking site visit. Please try again."
        });
        console.error(err);
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }
}
