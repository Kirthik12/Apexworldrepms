import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NgFor, NgIf, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface AmortRow {
  year: number;
  principal: number;
  interest: number;
  total: number;
  balance: number;
}

interface BankRate {
  name: string;
  rate: string;
  type: string;
}

@Component({
  selector: 'app-buyer-loans',
  templateUrl: './buyer-loans.component.html',
  styleUrls: ['./buyer-loans.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [RouterLink, NgFor, NgIf, DecimalPipe, FormsModule],
})
export class BuyerLoansComponent implements OnInit {

  // Inputs
  loanAmount: number = 5000000;
  tenureYears: number = 20;
  interestRate: number = 8.6;

  // Results
  emiResult: number = 0;
  totalInterest: number = 0;
  totalPayable: number = 0;
  principalPct: number = 0;

  // Donut chart values
  totalArc: number = 314; // circumference of r=50: 2 * Math.PI * 50
  principalArc: number = 0;
  donutOffset: number = 0;

  // Amortization
  showAmort: boolean = false;
  amortSchedule: AmortRow[] = [];

  // Bank rates reference
  bankRates: BankRate[] = [
    { name: 'State Bank of India', rate: '8.50', type: 'Floating' },
    { name: 'HDFC Bank', rate: '8.70', type: 'Floating' },
    { name: 'ICICI Bank', rate: '8.75', type: 'Floating' },
    { name: 'Axis Bank', rate: '8.75', type: 'Floating' },
    { name: 'Kotak Mahindra Bank', rate: '8.65', type: 'Floating' },
    { name: 'Punjab National Bank', rate: '8.45', type: 'Floating' },
  ];

  ngOnInit(): void {
    this.calculate();
  }

  calculate(): void {
    const P = Number(this.loanAmount) || 0;
    const r = (Number(this.interestRate) || 8.6) / 12 / 100;
    const n = (Number(this.tenureYears) || 20) * 12;

    if (P <= 0 || r <= 0 || n <= 0) {
      this.emiResult = 0;
      this.totalInterest = 0;
      this.totalPayable = 0;
      return;
    }

    const emi = (P * r * Math.pow(1 + r, n)) / (Math.pow(1 + r, n) - 1);
    this.emiResult = Math.round(emi);
    this.totalPayable = Math.round(emi * n);
    this.totalInterest = this.totalPayable - P;

    // Donut chart
    this.principalPct = (P / this.totalPayable) * 100;
    const circumference = 2 * Math.PI * 50;
    this.totalArc = circumference;
    this.principalArc = (P / this.totalPayable) * circumference;
    this.donutOffset = 0;

    // Build amortization schedule (year-wise)
    this.buildAmortization(P, r, emi, n);
  }

  buildAmortization(P: number, r: number, emi: number, totalMonths: number): void {
    this.amortSchedule = [];
    let balance = P;
    const years = Math.ceil(totalMonths / 12);

    for (let y = 1; y <= years; y++) {
      const months = y < years ? 12 : totalMonths - (years - 1) * 12;
      let yearPrincipal = 0;
      let yearInterest = 0;

      for (let m = 0; m < months; m++) {
        const interestPayment = balance * r;
        const principalPayment = emi - interestPayment;
        yearInterest += interestPayment;
        yearPrincipal += principalPayment;
        balance = Math.max(0, balance - principalPayment);
      }

      this.amortSchedule.push({
        year: y,
        principal: Math.round(yearPrincipal),
        interest: Math.round(yearInterest),
        total: Math.round(yearPrincipal + yearInterest),
        balance: Math.round(balance),
      });
    }
  }
}
