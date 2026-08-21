import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-help-center',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './admin-help-center.component.html',
  styleUrls: ['./admin-help-center.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class AdminHelpCenterComponent implements OnInit {
  faqs: { question: string; answer: string; active?: boolean }[] = [];
  
  defaultFaqs = [
    { question: "How do I register a new property listing?", answer: "Navigate to Property Management and click 'Add Property'. Fill in the required details including location, price, and images, then submit to make it live.", active: false },
    { question: "How do I approve a buyer's loan application?", answer: "Go to Loan Management, locate the buyer's pending application, review their EMI plans and uploaded documents, and click 'Approve'.", active: false },
    { question: "What happens to a booking when payment fails?", answer: "The booking status remains 'Pending'. You can track this in Booking Management and follow up with the buyer. If the payment gateway failed during UPI, they can retry using the manual UPI flow.", active: false },
    { question: "How do I generate a revenue forecast report?", answer: "Go to Reports & Analytics, select 'Revenue Forecast' from the template dropdown, specify your date range, and click Generate.", active: false },
    { question: "How do I restore a database backup?", answer: "Open Backup & Recovery, locate the snapshot you wish to restore from the history table, and click the 'Restore' action icon.", active: false }
  ];

  constructor() {}

  ngOnInit(): void {
    this.loadFaqs();
  }

  loadFaqs(): void {
    const saved = localStorage.getItem("hc_faqs_v2");
    if (saved) {
      this.faqs = JSON.parse(saved).map((f: any) => ({
        question: typeof f === 'string' ? f : (f.question || "Untitled Question"),
        answer: typeof f === 'string' ? "Answer will be updated soon." : (f.answer || "No answer provided."),
        active: false
      }));
    } else {
      this.faqs = [...this.defaultFaqs];
      localStorage.setItem("hc_faqs_v2", JSON.stringify(this.defaultFaqs));
    }
  }

  toggleFaq(index: number): void {
    const isActive = this.faqs[index].active;
    this.faqs.forEach(f => f.active = false); // Close all
    this.faqs[index].active = !isActive; // Toggle current
  }

  createFaq(question: string, answer: string): void {
    if (!question || !answer) return;
    const newFaq = { question, answer, active: false };
    this.faqs.unshift(newFaq);
    localStorage.setItem("hc_faqs_v2", JSON.stringify(this.faqs.map(f => ({ question: f.question, answer: f.answer }))));
  }
}
