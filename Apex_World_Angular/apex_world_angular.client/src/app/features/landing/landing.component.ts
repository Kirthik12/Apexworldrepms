import { Component, OnInit, AfterViewInit, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import { runVanillaLogic } from '../../app-logic';
import { EnquiryService, CreateEnquiryDto } from '../../core/services/enquiry.service';
import { SystemService } from '../../core/services/system.service';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-landing',
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.css'],
  encapsulation: ViewEncapsulation.None,
  imports: [RouterLink, FormsModule, NgIf],
})
export class LandingComponent implements OnInit, AfterViewInit {
  enquiryData: CreateEnquiryDto = {
    name: '',
    email: '',
    phone: '',
    subject: 'General Enquiry from Landing Page',
    message: 'Please contact me regarding property investments.',
  };
  isRobotCheck: boolean = false;
  isSubmittingEnquiry: boolean = false;
  enquirySuccessMsg: string = '';
  enquiryErrorMsg: string = '';

  constructor(
    private enquiryService: EnquiryService,
    private systemService: SystemService,
    private cdr: ChangeDetectorRef
  ) {
    // Expose methods to global scope so that existing inline HTML handlers (like onclick) still work perfectly.
    (window as any).dismissTopBanner = this.dismissTopBanner.bind(this);
    (window as any).showCopyToast = this.showCopyToast.bind(this);
  }

  ngOnInit() {}

  ngAfterViewInit() {
    setTimeout(() => {
      runVanillaLogic();
    }, 100);
    const defData = {
      hero: {
        title: 'Find Your Perfect <span>Real Estate</span> Property',
        sub: 'Discover premium apartments, villas, and commercial spaces tailored to your lifestyle across prime locations.',
      },
      categories: [
        { img: 'assets/images/apartments.png', name: 'Apartments', count: '1,240' },
        { img: 'assets/images/villas.png', name: 'Luxury Villas', count: '320' },
        { img: 'assets/images/prop_sunrise_studio.png', name: 'Studio Flats', count: '485' },
        { img: 'assets/images/commercial.png', name: 'Commercials Space', count: '115' },
      ],
      services: [
        {
          icon: '📄',
          title: 'Property Listings',
          desc: 'Buy, sell & rent • all properties are 100% verified by our real estate experts.',
        },
        {
          icon: '🏦',
          title: 'Home Loans',
          desc: 'Compare premium loan rates from top public & private banks at low EMIs.',
        },
        {
          icon: '⚖️',
          title: 'Legal Assistance',
          desc: 'Documentation verification and complete paperwork handled by our legal team.',
        },
        {
          icon: '📈',
          title: 'Property Valuation',
          desc: 'Get instant and highly accurate property valuation reports free of cost.',
        },
      ],
      stats: [
        { icon: '💰', prefix: '₹ ', val: '8.5', suffix: ' Cr', label: 'Revenue Generated' },
        { icon: '🏢', prefix: '', val: '1248', suffix: '+', label: 'Properties Listed' },
        { icon: '😊', prefix: '', val: '3840', suffix: '+', label: 'Satisfied Buyers' },
      ],
      testimonials: [
        {
          initials: 'PS',
          name: 'Priya Sharma',
          role: '⭐ Verified Buyer',
          text: 'Found my dream apartment in Guindy within just 2 weeks. The documentation process was extremely transparent and stress-free.',
        },
        {
          initials: 'RK',
          name: 'Rajesh Kumar',
          role: '⭐ Property Investor',
          text: 'Apex World provided excellent ROI projections for commercial spaces. Highly professional and data-driven approach.',
        },
        {
          initials: 'MN',
          name: 'Meera Nair',
          role: '⭐ First-time Buyer',
          text: 'The home loan assistance was a lifesaver. They negotiated a great interest rate and handled all bank interactions.',
        },
      ],
    };

    let content: any = defData;
    this.systemService.getPublicContents().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          const sections: Record<string, any[]> = {};
          res.data.forEach((item: any) => {
            const secName = (item.section || 'General').toLowerCase().trim();
            if (!sections[secName]) sections[secName] = [];
            sections[secName].push(item);
          });

          // 1. Hero Section
          if (sections['hero']) {
            if (!content.hero) content.hero = {};
            sections['hero'].forEach(item => {
              const lowerKey = item.key.toLowerCase();
              if (lowerKey === 'title' || lowerKey === 'headline') content.hero.title = item.value;
              if (lowerKey === 'sub' || lowerKey === 'subheadline') content.hero.sub = item.value;
            });
          }

          // 2. Categories Section
          if (sections['categories'] && sections['categories'].length > 0) {
            content.categories = sections['categories'].map(item => {
              let details = {};
              try { details = JSON.parse(item.value); } catch(e){}
              return { name: item.key, ...details };
            });
          }

          // 3. Services Section
          if (sections['services'] && sections['services'].length > 0) {
            content.services = sections['services'].map(item => {
              let details = {};
              try { details = JSON.parse(item.value); } catch(e){}
              return { title: item.key, ...details };
            });
          }

          // 4. Stats Section
          if (sections['stats'] && sections['stats'].length > 0) {
            content.stats = sections['stats'].map(item => {
              let details = {};
              try { details = JSON.parse(item.value); } catch(e){}
              return { label: item.key, ...details };
            });
          }

          // 5. Testimonials Section
          if (sections['testimonials'] && sections['testimonials'].length > 0) {
            content.testimonials = sections['testimonials'].map(item => {
              let details = {};
              try { details = JSON.parse(item.value); } catch(e){}
              return { name: item.key, ...details };
            });
          }

          // 6. Enquiry Section
          if (sections['enquiry']) {
            if (!content.enquiry) content.enquiry = {};
            sections['enquiry'].forEach(item => {
              const lowerKey = item.key.toLowerCase();
              if (lowerKey === 'title') content.enquiry.title = item.value;
              if (lowerKey === 'desc' || lowerKey === 'description') content.enquiry.desc = item.value;
            });
          }

          // 7. Contacts Section
          if (sections['contacts']) {
            if (!content.contacts) content.contacts = {};
            sections['contacts'].forEach(item => {
              const lowerKey = item.key.toLowerCase();
              if (lowerKey === 'address') content.contacts.address = item.value;
              if (lowerKey === 'phone') content.contacts.phone = item.value;
              if (lowerKey === 'email') content.contacts.email = item.value;
            });
          }

          // 8. General (Backward compatibility)
          if (sections['general']) {
            sections['general'].forEach(item => {
              const lowerKey = item.key.toLowerCase();
              if (lowerKey === 'headline') {
                if (!content.hero) content.hero = {};
                content.hero.title = item.value;
              } else if (lowerKey === 'subheadline') {
                if (!content.hero) content.hero = {};
                content.hero.sub = item.value;
              } else {
                const trimmed = item.value.trim();
                if (trimmed.startsWith('{') || trimmed.startsWith('[')) {
                  try {
                    content[item.key] = JSON.parse(item.value);
                  } catch (e) {
                    content[item.key] = item.value;
                  }
                } else {
                  content[item.key] = item.value;
                }
              }
            });
          }
        }
        try {
          localStorage.setItem('landing_content', JSON.stringify(content));
        } catch (e) {}
        this.renderLandingPage(content);
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error fetching landing content', err);
        try {
          const raw = localStorage.getItem('landing_content');
          if (raw) {
            content = JSON.parse(raw);
          }
        } catch (e) {}
        this.renderLandingPage(content);
        this.cdr.detectChanges();
      }
    });
  }

  renderLandingPage(content: any) {
    let patched = false;
    if (content.categories) {
      content.categories.forEach((c: any) => {
        if (c.img === 'assets/images/apartment.png') {
          c.img = 'assets/images/apartments.png';
          patched = true;
        }
        if (c.img === 'assets/images/villa.png') {
          c.img = 'assets/images/villas.png';
          patched = true;
        }
        if (c.img === 'assets/images/studio.png') {
          c.img = 'assets/images/prop_sunrise_studio.png';
          patched = true;
        }
      });
    }
    if (patched) {
      try {
        localStorage.setItem('landing_content', JSON.stringify(content));
      } catch (e) {}
    }

    const hTitle = document.getElementById('dyn-hero-title');
    if (hTitle && content.hero) hTitle.innerHTML = content.hero.title;
    const hSub = document.getElementById('dyn-hero-sub');
    if (hSub && content.hero) hSub.innerHTML = content.hero.sub;

    const catDiv = document.getElementById('dyn-categories');
    if (catDiv && content.categories) {
      const filteredCategories = content.categories.filter(
        (c: any) => !c.name.toLowerCase().includes('penthouse'),
      );
      catDiv.innerHTML = filteredCategories
        .map(
          (c: any) => `
        <div class="category-card" tabindex="0" aria-label="Explore ${c.name} listings" onclick="event.preventDefault(); event.stopPropagation(); document.getElementById('category-gateway-modal').style.display='flex';">
          <div class="category-image-container">
            <img src="${c.img}" alt="${c.name}">
          </div>
          <div class="category-name">${c.name}</div>
          <div class="category-listings">${c.count} Listings</div>
        </div>
      `,
        )
        .join('');
    }

    const srvDiv = document.getElementById('dyn-services');
    if (srvDiv && content.services) {
      srvDiv.innerHTML = content.services
        .map(
          (s: any) => `
        <div class="service-card" tabindex="0" aria-label="Our service: ${s.title}">
          <div class="service-icon-box">${s.icon}</div>
          <h3>${s.title}</h3>
          <p>${s.desc}</p>
          <a href="javascript:void(0);" onclick="event.preventDefault(); event.stopPropagation(); (window as any).showServiceDetails(this);" class="service-link">Learn More &rarr;</a>
        </div>
      `,
        )
        .join('');
    }

    const statDiv = document.getElementById('dyn-stats');
    if (statDiv && content.stats) {
      statDiv.innerHTML = content.stats
        .map(
          (s: any) => `
        <div class="stat-item" tabindex="0">
          <span class="icon">${s.icon}</span>
          <span><span class="accent counter-val" data-target="${s.val}" data-prefix="${s.prefix}" data-suffix="${s.suffix}">${s.prefix}0${s.suffix}</span> ${s.label}</span>
        </div>
      `,
        )
        .join('');
    }

    const testDiv = document.getElementById('dyn-testimonials');
    if (testDiv && content.testimonials) {
      testDiv.innerHTML = content.testimonials
        .map(
          (t: any, index: number) => `
        <div class="testimonial-card testimonial-slide" style="display: ${index === 0 ? 'block' : 'none'};" data-index="${index}">
          <div class="testimonial-header">
            <div class="testimonial-avatar" aria-label="${t.name} Avatar">${t.initials}</div>
            <div>
              <div class="testimonial-name">${t.name}</div>
              <div class="testimonial-role">${t.role}</div>
            </div>
          </div>
          <p class="testimonial-comment">"${t.text}"</p>
        </div>
      `,
        )
        .join('');

      const dotsDiv = document.getElementById('test-dots');
      if (dotsDiv) {
        dotsDiv.innerHTML = content.testimonials
          .map(
            (t: any, idx: number) => `
          <div class="hero-dot${idx === 0 ? ' active' : ''}" data-test-index="${idx}" role="button" aria-label="Testimonial ${idx + 1}"></div>
        `,
          )
          .join('');
      }
    }

    const contentEnquiry = (content as any).enquiry;
    if (contentEnquiry) {
      const enqTitle = document.querySelector('#enquiry-desk h2');
      if (enqTitle) enqTitle.innerHTML = contentEnquiry.title;
      const enqDesc = document.querySelector('#enquiry-desk p');
      if (enqDesc) enqDesc.innerHTML = contentEnquiry.desc;
    }

    const contentContacts = (content as any).contacts;
    if (contentContacts) {
      const contactCol = document.getElementById('contacts');
      if (contactCol) {
        contactCol.innerHTML = `
          <h4>Contacts & Office</h4>
          <p style="font-weight: 600; margin-bottom: 6px;">Apex World HQ</p>
          <p>${contentContacts.address.replace(/\n/g, '<br>')}</p>
          <p style="font-size: 0.9rem; margin-top: 10px; margin-bottom: 4px;">📞 ${contentContacts.phone}</p>
          <p style="font-size: 0.9rem; margin-bottom: 4px;">✉️ ${contentContacts.email}</p>
        `;
      }
    }

    const slides = document.querySelectorAll('.top-offer-slide');
    let current = 0;
    if (slides.length > 0) {
      setInterval(() => {
        (slides[current] as HTMLElement).style.opacity = '0';
        (slides[current] as HTMLElement).style.pointerEvents = 'none';
        current = (current + 1) % slides.length;
        (slides[current] as HTMLElement).style.opacity = '1';
        (slides[current] as HTMLElement).style.pointerEvents = 'auto';
      }, 6000);
    }

    if (typeof (window as any).initializePremiumFeatures === 'function') {
      (window as any).initializePremiumFeatures();
    }

    const contactsLink = document.getElementById('nav-contacts-link');
    const contactsModal = document.getElementById('contacts-modal');
    if (contactsLink && contactsModal) {
      contactsLink.addEventListener('click', (e) => {
        e.preventDefault();
        e.stopPropagation();
        contactsModal.style.display = 'flex';
      });
    }
  }

  dismissTopBanner() {
    const banner = document.getElementById('top-offers-banner');
    if (banner) {
      banner.style.opacity = '0';
      banner.style.transform = 'translateY(-100%)';
      setTimeout(() => {
        banner.style.display = 'none';
        document.body.style.paddingTop = '70px';
      }, 3000);
    }
  }

  submitEnquiry() {
    if (!this.isRobotCheck) {
      this.enquiryErrorMsg = "Please check the 'I'm not a robot' box.";
      return;
    }
    if (!this.enquiryData.name || !this.enquiryData.email || !this.enquiryData.phone) {
      this.enquiryErrorMsg = 'Please fill in all details.';
      return;
    }

    this.isSubmittingEnquiry = true;
    this.enquiryErrorMsg = '';
    this.enquirySuccessMsg = '';

    this.enquiryService.submitEnquiry(this.enquiryData).subscribe({
      next: () => {
        this.enquirySuccessMsg =
          "Thank you! Your enquiry has been submitted. We'll contact you shortly.";
        this.enquiryData = {
          name: '',
          email: '',
          phone: '',
          subject: 'General Enquiry',
          message: '',
        };
        this.isRobotCheck = false;
        this.isSubmittingEnquiry = false;
      },
      error: () => {
        this.enquiryErrorMsg = 'Something went wrong. Please try again.';
        this.isSubmittingEnquiry = false;
      },
    });
  }

  showCopyToast(btn: HTMLElement) {
    const originalText = btn.textContent;
    btn.textContent = 'Copied!';
    btn.style.background = '#10B981';
    btn.style.color = '#fff';
    setTimeout(() => {
      btn.textContent = originalText || '';
      btn.style.background = 'var(--secondary-light)';
      btn.style.color = 'var(--secondary-dark)';
    }, 1500);
  }
}
