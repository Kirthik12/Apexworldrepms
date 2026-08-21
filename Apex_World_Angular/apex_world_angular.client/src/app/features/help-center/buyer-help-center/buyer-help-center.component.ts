import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgIf, NgFor } from '@angular/common';

interface FAQ {
  id: number;
  question: string;
  answer: string;
  expanded: boolean;
  category: string;
}

@Component({
  selector: 'app-buyer-help-center',
  templateUrl: './buyer-help-center.component.html',
  styleUrls: ['./buyer-help-center.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [FormsModule, NgIf, NgFor],
})
export class BuyerHelpCenterComponent implements OnInit {
  searchQuery: string = '';

  faqs: FAQ[] = [
    {
      id: 1,
      category: 'Account',
      question: 'How do I change my password?',
      answer:
        'To change your password, go to the "Dashboard" dropdown menu in the top right corner and click "Change Password". Enter your current password and your new password to update it.',
      expanded: false,
    },
    {
      id: 2,
      category: 'Property',
      question: 'How do I add a property to my wishlist?',
      answer:
        'When browsing properties in the Property Explorer, click the heart icon on any property card to save it to your wishlist. You can view all saved properties in the "My Wishlist" section.',
      expanded: false,
    },
    {
      id: 3,
      category: 'Booking',
      question: 'Can I cancel a site visit?',
      answer:
        'Yes, you can cancel a scheduled site visit from the "Reservation Desk" page. Find your booking in the list and click the "Cancel Booking" button.',
      expanded: false,
    },
    {
      id: 4,
      category: 'Payment',
      question: 'What payment methods do you accept for token advances?',
      answer:
        'We currently accept Credit/Debit cards, Net Banking, and UPI for paying token advances on properties.',
      expanded: false,
    },
    {
      id: 5,
      category: 'Loan',
      question: 'How does the EMI Calculator work?',
      answer:
        'The EMI Calculator helps you estimate your monthly loan payments. Enter the loan amount, interest rate, and loan tenure, and it will automatically calculate your estimated monthly installment.',
      expanded: false,
    },
  ];

  filteredFaqs: FAQ[] = [];

  constructor() {}

  ngOnInit(): void {
    this.filteredFaqs = [...this.faqs];
  }

  toggleFaq(faq: FAQ): void {
    faq.expanded = !faq.expanded;
  }

  onSearch(): void {
    const query = this.searchQuery.toLowerCase().trim();
    if (!query) {
      this.filteredFaqs = [...this.faqs];
      return;
    }

    this.filteredFaqs = this.faqs.filter(
      (faq) =>
        faq.question.toLowerCase().includes(query) ||
        faq.answer.toLowerCase().includes(query) ||
        faq.category.toLowerCase().includes(query),
    );
  }

  filterByCategory(category: string): void {
    this.searchQuery = category;
    this.onSearch();
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.onSearch();
  }
}
