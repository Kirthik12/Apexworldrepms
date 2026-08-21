import { Component, AfterViewInit, ViewEncapsulation, OnDestroy } from '@angular/core';
import { BookingService } from '../../../core/services/booking.service';
import { PaymentService } from '../../../core/services/payment.service';
import { ToastService } from '../../../core/services/toast.service';
import { ActivatedRoute, Router } from '@angular/router';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-buyer-bookings',
  templateUrl: './buyer-bookings.component.html',
  styleUrls: ['./buyer-bookings.component.css'],
  encapsulation: ViewEncapsulation.None,
})
export class BuyerBookingsComponent implements AfterViewInit, OnDestroy {
  backendUrl = environment.apiUrl.replace('/api/v1', '');
  private timerInterval: any;

  constructor(
    private bookingService: BookingService,
    private route: ActivatedRoute,
    private router: Router,
    private paymentService: PaymentService,
    private toastService: ToastService
  ) {}

  ngAfterViewInit() {
    const self = this;
    // ── Exclusive Deals top bar: rotate slides every 3s
    const slides = Array.from(document.querySelectorAll('.top-offer-slide')) as HTMLElement[];
    let current = 0;
    if (slides.length > 0) {
      setInterval(() => {
        slides[current].style.opacity = '0';
        slides[current].style.pointerEvents = 'none';
        current = (current + 1) % slides.length;
        slides[current].style.opacity = '1';
        slides[current].style.pointerEvents = 'auto';
      }, 3000);
    }
    const dismissBtn = document.querySelector('.top-bar button');
    if (dismissBtn) {
      dismissBtn.addEventListener('click', () => {
        const b = document.getElementById('top-offers-banner');
        if (b) b.style.display = 'none';
      });
    }

    const w = window as any;

    if (localStorage.getItem('bookings_cleared_v1') !== 'true') {
      if (typeof w.DB !== 'undefined') w.DB.save('bookings', []);
      localStorage.removeItem('nb_bookings');
      localStorage.setItem('bookings_cleared_v1', 'true');
      console.log('Bookings have been cleared from local storage.');
    }

    

    const STORAGE_KEY = 'nb_bookings';
    const SEED_BOOKINGS = [
      {
        id: 'AWB123456',
        name: 'Skyline Heights',
        loc: 'Besant Nagar, Chennai',
        type: 'Luxury Apartment',
        size: '1,650 sq.ft',
        beds: '3 Beds',
        baths: '3 Baths',
        price: '₹ 82,00,000',
        date: '20 May 2026',
        status: 'Agreement Signed',
        tab: 'signed',
        agent: 'Suresh Kumar — +91 98765 43210',
        img: '/assets/images/skyline_heights.png',
        paid: false,
        cancelled: false,
        timeline: ['done', 'done', '', ''],
      },
      {
        id: 'AWB123457',
        name: 'Green Meadows Villa',
        loc: 'Adyar, Chennai',
        type: 'Luxury Villa',
        size: '4,650 sq.ft',
        beds: '8 Beds',
        baths: '10 Baths',
        price: '₹ 2,45,00,000',
        date: '18 May 2026',
        status: 'Site Visit Scheduled',
        tab: 'scheduled',
        agent: 'Priya Nair — +91 98765 11111',
        img: '/assets/images/green_meadows.png',
        paid: false,
        cancelled: false,
        visitDate: '24 May 2026, 11:00 AM',
        timeline: ['done', 'done', 'active', ''],
      },
      {
        id: 'AWB123458',
        name: 'Marina Enclave',
        loc: 'Marina Beach Road, Chennai',
        type: 'Sea-View Apartment',
        size: '1,350 sq.ft',
        beds: '2 Beds',
        baths: '2 Baths',
        price: '₹ 65,00,000',
        date: '15 May 2026',
        status: 'Payment Pending',
        tab: 'pending',
        agent: 'Ravi Shankar — +91 98765 22222',
        img: '/assets/images/marina_enclave.png',
        paid: true,
        cancelled: false,
        balanceDue: '₹ 25,00,000',
        dueDate: '25 May 2026',
        timeline: ['done', '', 'active', ''],
      },
      {
        id: 'AWB123459',
        name: 'Palm Grove Villa',
        loc: 'ECR, Chennai',
        type: 'Premium Villa',
        size: '5,550 sq.ft',
        beds: '5 BHK',
        baths: '6 Baths',
        price: '₹ 4₹0,00,000',
        date: '10 May 2026',
        status: 'Completed',
        tab: 'completed',
        agent: 'Meena Pillai — +91 98765 33333',
        img: '/assets/images/palm_grove.png',
        paid: true,
        cancelled: false,
        timeline: ['done', 'done', 'done', 'done'],
      },
    ];

    const mapBackendBookings = (backendData: any[]): any[] => {
      return backendData.map((b) => {
        const prop = b.property || {};
        const cat = prop.category || {};
        const formattedDate = new Date(b.createdAt || new Date()).toLocaleDateString('en-IN', {
          day: 'numeric',
          month: 'short',
          year: 'numeric',
        });

        // Map backend Status to UI tab/status
        let tab = 'signed';
        let timeline = ['done', '', '', ''];

        if (b.status === 'Cancelled' || b.status === 'Rejected') {
          tab = 'cancelled';
        } else if (b.status === 'PendingAdminApproval') {
          tab = 'scheduled';
          timeline = ['done', '', '', ''];
        } else if (b.status === 'Approved') {
          tab = 'scheduled';
          timeline = ['done', '', '', ''];
        } else if (b.status === 'Visited') {
          tab = 'scheduled';
          timeline = ['done', 'done', '', ''];
        } else if (b.status === 'Interested') {
          tab = 'scheduled';
          timeline = ['done', 'done', 'active', ''];
        } else if (b.status === 'Paid') {
          tab = 'completed';
          timeline = ['done', 'done', 'done', 'done'];
        } else {
          tab = 'pending';
        }

        let formattedPrice = `₹ ${prop.price?.toLocaleString('en-IN') || '0'}`;

        return {
          id: b.id.toString(),
          rawId: b.id,
          propId: prop.id,
          name: prop.title || 'Unknown Property',
          loc: prop.address || 'Unknown Location',
          type: cat.name || 'Property',
          size: `${prop.areaSize || 0} sq.ft`,
          beds: `${prop.bedrooms || 0} Beds`,
          baths: `${prop.bathrooms || 0} Baths`,
          price: formattedPrice,
          date: formattedDate,
          createdAtRaw: b.createdAt,
          status: b.status,
          tab: tab,
          agent: 'Apex Admin — +91 99999 99999',
          img: prop.images && prop.images.length > 0 ? prop.images[0].imageUrl : null,
          paid: b.status === 'Paid',
          cancelled: b.status === 'Cancelled' || b.status === 'Rejected',
          visitDate: b.scheduledDate
            ? new Date(b.scheduledDate).toLocaleDateString('en-IN', {
                day: 'numeric',
                month: 'short',
                year: 'numeric',
              })
            : null,
          visitSlot: b.scheduledDate
            ? new Date(b.scheduledDate).toLocaleTimeString('en-IN', {
                hour: '2-digit',
                minute: '2-digit',
              })
            : null,
          visitStatus:
            b.status === 'Rejected' ? 'denied' : (b.status === 'Approved' ? 'approved' : 'pending'),
          visitDenialReason: b.rejectionReason,
          timeline: timeline,
        };
      });
    };

    function saveBookings(list: any[]) {
      // Local storage save logic removed, since we're using the DB now.
    }

    function cancelBookingById(id: string) {
      if (!confirm('Are you sure you want to cancel this booking?')) return [];

      self.bookingService.cancelBooking(parseInt(id, 10)).subscribe({
        next: () => {
          self.toastService.success('Booking cancelled successfully.');
          renderBookings();
        },
        error: (err: any) => {
          console.error(err);
          self.toastService.error('Failed to cancel booking.');
        },
      });
      return [];
    }

    const listContainer = document.getElementById('bookings-list-container');
    const emptyState = document.getElementById('bk-empty-state');

    function statusBadgeClass(tab: string) {
      const map: any = {
        signed: 'bk-badge-signed',
        scheduled: 'bk-badge-scheduled',
        pending: 'bk-badge-pending',
        completed: 'bk-badge-completed',
        cancelled: 'bk-badge-cancelled',
        'loan-applied': 'bk-badge-loan',
      };
      return map[tab] || 'bk-badge-signed';
    }
    function cardClass(tab: string) {
      const map: any = {
        pending: 'bk-card-danger',
        completed: 'bk-card-success',
        cancelled: 'bk-card-cancelled',
        'loan-applied': 'bk-card-loan',
      };
      return map[tab] || '';
    }
    function statusLabel(b: any) {
      if (b.status === 'Completed (EMI)') return '🏦 EMI Active';
      if (b.status === 'Waiting for Bank Approval') return '🏦 Loan Under Review';
      const map: any = {
        signed: '✔ Agreement Signed',
        scheduled: '🗓️ Site Visit',
        pending: '⚠ Payment Pending',
        completed: '🎉 Completed',
        cancelled: '🚫 Cancelled',
        'loan-applied': '🏦 Loan Under Review',
      };
      return map[b.tab] || b.status;
    }
    function timelineHTML(tl: any) {
      const labels = ['Site-Visit', 'Visited', 'Payment', 'Complete'];
      let html = '';
      const safeTl = tl || ['done', '', '', ''];
      safeTl.slice(0, 4).forEach((state: any, i: any) => {
        html += `<div class="bk-tl-step ${state}"><span class="bk-tl-dot"></span><span class="bk-tl-label">${labels[i]}</span></div>`;
        if (i < 3)
          html += `<div class="bk-tl-line ${safeTl[i] === 'done' && safeTl[i + 1] ? 'done' : ''}"></div>`;
      });
      return html;
    }

    function buildCard(b: any) {
      const isCancelled = b.cancelled || b.tab === 'cancelled';
      const canCancel = !isCancelled && b.tab !== 'registered';
      const priceNum = parseInt((b.price || '0').replace(/[^\d]/g, ''), 10);

      let extraBanner = '';

      if (b.visitDate && !isCancelled && !b.paid && b.tab !== 'completed') {
        if (b.visitStatus === 'denied') {
          extraBanner += `
              <div class="bk-visit-banner" style="margin-bottom: 8px; background:#FEF2F2; border:1px solid #FECACA; color:#991B1B;">
                <span>🚫 <strong>Admin has denied your site visit request.</strong><br>Reason: ${b.visitDenialReason || 'Not provided.'} Please search for other properties.</span>
              </div>`;
        } else if (b.visitStatus === 'approved' && b.status === 'Approved') {
          extraBanner += `
              <div class="bk-visit-banner" style="margin-bottom: 8px; background:#F8FAFC; border:1px solid #E2E8F0; color:#475569;">
                <span>🗓️ Site Visit: <strong>${b.visitDate}</strong> at ${b.visitSlot || ''} ? Agent: ${b.agent ? b.agent.split('—')[1]?.trim() || '' : ''}</span>
                <button class="bk-reschedule-btn reschedule-visit-btn" data-id="${b.id}" data-action="site-visit">Details & Reschedule</button>
                <button class="bk-btn-primary mark-visit-complete-btn" data-id="${b.id}" style="padding:4px 12px; font-size:0.8rem; margin-left: 8px;">Mark Visited</button>
              </div>`;
        } else if (b.status === 'PendingAdminApproval') {
          extraBanner += `
              <div class="bk-visit-banner" style="margin-bottom: 8px; background:#F8FAFC; border:1px solid #E2E8F0; color:#475569;">
                <span>⏳ Site Visit: <strong>${b.visitDate}</strong> at ${b.visitSlot || ''}. <br><strong>Awaiting Admin Approval for Site Visit.</strong></span>
                  <button class="bk-btn-primary" disabled style="padding:4px 12px; font-size:0.8rem; margin-left: 8px; background:transparent; color:#64748B; font-weight:600; border:none; cursor:not-allowed;">Awaiting Approval</button>
              </div>`;
        }
      }

      if (!isCancelled && b.tab !== 'completed' && !b.paid) {
        if (b.status === 'Visited') {
          extraBanner += `
            <div class="bk-payment-banner" style="background:#ECFDF5; border-color:#10B981; display:flex; justify-content:space-between; align-items:center;">
              <span>👀 <strong>Site Visit Completed!</strong> Are you interested in purchasing this property? <br><span class="countdown-timer" data-id="${b.id}" style="color:#EF4444; font-weight:700; font-size:0.85rem;"></span></span>
              <div style="display:flex; gap:10px;">
                  <button class="pass-prop-btn" data-id="${b.id}" style="background:transparent; color:#EF4444; border:1px solid #EF4444; padding:6px 14px; border-radius:6px; cursor:pointer; font-weight:600; font-size:0.85rem; transition:0.2s;">Not Interested</button>
                  <button class="btn-interested-direct-btn" data-id="${b.id}" style="background:transparent; color:#10B981; border:1px solid #10B981; padding:6px 14px; border-radius:6px; cursor:pointer; font-weight:600; font-size:0.85rem; transition:0.2s;">Interested</button>
              </div>
            </div>`;
        } else if (b.status === 'Interested') {
          extraBanner += `
            <div class="bk-payment-banner" style="background:#ECFDF5; border-color:#10B981; display:flex; justify-content:space-between; align-items:center;">
              <span>💳 <strong>Interest Logged!</strong> You can now proceed to purchase this property. <br><span class="countdown-timer" data-id="${b.id}" style="color:#EF4444; font-weight:700; font-size:0.85rem;"></span></span>
              <div style="display:flex; gap:10px;">
                  <button class="pass-prop-btn" data-id="${b.id}" style="background:transparent; color:#EF4444; border:1px solid #EF4444; padding:6px 14px; border-radius:6px; cursor:pointer; font-weight:600; font-size:0.85rem; transition:0.2s;">Pass</button>
                  <button style="background:transparent; color:#10B981; border:1px solid #10B981; padding:6px 14px; border-radius:6px; cursor:pointer; font-weight:600; font-size:0.85rem; transition:0.2s;" onclick="window.location.href='/buyer-dashboard/payment-management?id=${encodeURIComponent(b.id)}'">Acquire</button>
              </div>
            </div>`;
        } else if (b.status === 'Waiting for Bank Approval' || b.tab === 'loan-applied') {
          extraBanner += `
            <div class="bk-payment-banner" style="background:#EFF6FF; border-color:#3B82F6; flex-direction:column; gap:10px; padding: 14px 16px;">
              <div style="display:flex; justify-content:space-between; align-items:flex-start;">
                <span style="font-size:0.92rem;">
                  🏦 <strong>Loan Application Submitted!</strong><br>
                  <span style="font-size:0.82rem; color:#475569; line-height:1.6;">
                    Your application has been submitted to <strong>${b.loanBank || 'the respective bank'}</strong>.
                    Please wait for their approval. Once the loan is approved,
                    <strong>you will be the owner of this property.</strong>
                    &nbsp; Ref: <code style="font-size:0.75rem; background:#DBEAFE; padding:2px 6px; border-radius:4px;">${b.loanRef || 'LOAN-XXXX'}</code>
                  </span>
                </span>
              </div>
              <div style="display:flex; gap:10px; align-items:center; flex-wrap:wrap;">
                <span style="font-size:0.75rem; color:#64748B;">⏳ Awaiting bank verification &amp; approval…</span>
                <button onclick="window.approveLoan('${b.id}')" style="background:#1D4ED8; color:#fff; border:none; padding:7px 18px; border-radius:6px; cursor:pointer; font-weight:700; font-size:0.82rem; white-space:nowrap;">✅ Simulate Loan Approval</button>
              </div>
            </div>`;
        }
      }

      const imgPath =
        b.img && b.img.startsWith('http')
          ? b.img
          : b.img
            ? self.backendUrl + b.img
            : '/assets/images/placeholder.jpg';
      const resolvedImg = imgPath;
      return `
        <div class="bk-card ${cardClass(b.tab)} ${isCancelled ? 'bk-card-cancelled-style' : ''}"
             data-tab="${b.tab}"
             data-name="${b.name}" data-loc="${b.loc}" data-type="${b.type}"
             data-size="${b.size}" data-beds="${b.beds}" data-baths="${b.baths}"
             data-price="${b.price}" data-id="${b.id}" data-date="${b.date}"
             data-status="${b.status}" data-agent="${b.agent}" data-img="${b.img}"
             data-booking-id="${b.id}" data-prop-id="${b.propId}"
             style="position: relative;">
          ${isCancelled ? `<button class="bk-delete-record-btn" title="Remove this record" data-id="${b.id}" onclick="event.stopPropagation(); window.deleteBookingRecord('${b.id}')" style="position:absolute;top:10px;right:10px;background:#EF4444;color:#fff;border:none;border-radius:50%;width:26px;height:26px;font-size:1rem;font-weight:700;cursor:pointer;display:flex;align-items:center;justify-content:center;z-index:10;box-shadow:0 2px 6px rgba(239,68,68,0.4);line-height:1;">&#10005;</button>` : ''}
          <div class="bk-card-img-wrap">
            <img src="${resolvedImg}" alt="${b.name}" class="bk-card-img" onerror="this.src='/assets/images/log✅png'">
            <div class="bk-card-badge ${statusBadgeClass(b.tab)}">${statusLabel(b)}</div>
          </div>
          <div class="bk-card-body">
            <div class="bk-card-meta">
              <span class="bk-card-id">ID: ${b.id}</span>
              <span class="bk-card-date">&#128197; ${b.date}</span>
            </div>
            <h3 class="bk-card-title">${b.name}</h3>
            <p class="bk-card-location">&#128205; ${b.loc}</p>
            <div class="bk-card-chips">
              <span class="bk-chip">&#127991;&#65039; ${b.type}</span>
              <span class="bk-chip">&#128208; ${b.size}</span>
              <span class="bk-chip">&#128719;&#65039; ${b.beds}</span>
              <span class="bk-chip">&#128699;&#65039; ${b.baths}</span>
            </div>
            <div class="bk-card-price-row">
              <div>
                <div class="bk-price-label">Total Price</div>
                <div class="bk-price">${b.price}</div>
              </div>
              <div class="bk-card-actions">
                <button class="bk-btn-outline view-details-btn">&#128269; View Details</button>
                ${
                  isCancelled
                    ? `<button class="bk-btn-status" style="background:#FEF2F2;color:#991B1B;border:1.5px solid #FECACA;" disabled>&#128683; Cancelled</button>`
                    : `<button class="bk-btn-status ${b.tab}" ${b.tab === 'scheduled' ? `data-action="site-visit"` : ''}>${statusLabel(b)}</button>`
                }
                ${canCancel ? `<button class="bk-btn-outline bk-btn-cancel cancel-booking-btn" data-paid="${b.paid}" data-name="${b.name}" data-price="${priceNum}" data-id="${b.id}">&#128683; Cancel Booking</button>` : ''}
              </div>
            </div>
            ${extraBanner}
            <div class="bk-timeline">${timelineHTML(b.timeline)}</div>
            ${isCancelled ? `<div class="bk-cancelled-note">&#9888;&#65039; This booking was cancelled. ${b.paid ? 'Refund in progress (5–7 working days).' : 'No charges applied.'}</div>` : ''}
          </div>
        </div>`;
    }

    w.deleteBookingRecord = function (id: string) {
      if (!confirm('Remove this cancelled booking record? This cannot be undone.')) return;

      self.bookingService.cancelBooking(parseInt(id, 10)).subscribe({
        next: () => {
          self.toastService.info('Cancelled booking record removed.');
          renderBookings();
        },
        error: (err: any) => {
          console.error(err);
          self.toastService.error('Failed to delete booking.');
        },
      });
    };

    w.approveLoan = function (id: string) {
      if (
        !confirm(
          'Simulate bank loan approval for this property? This will mark the payment as fully completed and you will become the owner.',
        )
      )
        return;
      const list = loadBookings();
      const idx = list.findIndex((b: any) => b.id === id);
      if (idx === -1) return;

      list[idx].status = 'Completed (EMI)';
      list[idx].tab = 'completed';
      list[idx].paid = true;
      list[idx].timeline = ['done', 'done', 'done', 'done'];

      saveBookings(list);
      self.toastService.success(
        '🎉 Loan approved! Congratulations — you are now the owner of this property.'
      );
      renderBookings();
    };

    let allBookings: any[] = [];

    function loadBookings() {
      return allBookings;
    }

    function renderBookings() {
      self.bookingService.getBuyerBookings().subscribe({
        next: (res: any) => {
          if (res.data) {
            allBookings = mapBackendBookings(res.data);

            if (!listContainer) return;

            const active = allBookings.filter((b: any) => !b.cancelled);
            const cancelled = allBookings.filter((b: any) => b.cancelled);
            const ordered = [...active, ...cancelled];

            listContainer.innerHTML = ordered.map(buildCard).join('');

            rebindEvents();
            updateStats(allBookings);
            applyFilters();
          }
        },
        error: (err: any) => {
          console.error('Failed to load bookings from API', err);
          self.toastService.error('Failed to load bookings');
        },
      });
    }

    function updateStats(bookings: any[]) {
      const active = bookings.filter((b: any) => !b.cancelled);
      const total = active.length;
      const pending = active.filter((b: any) => b.tab === 'pending').length;
      const completed = active.filter((b: any) => b.tab === 'completed').length;
      const inProgress = active.filter(
        (b: any) => b.tab !== 'completed' && b.tab !== 'pending',
      ).length;

      const set = (id: string, val: any) => {
        const el = document.getElementById(id);
        if (el) el.textContent = val;
      };
      set('stat-total', total);
      set('stat-pending', pending);
      set('stat-active', inProgress);
      set('stat-completed', completed);

      const tabs = document.querySelectorAll('.bk-tab');
      tabs.forEach((tab: any) => {
        const key = tab.getAttribute('data-tab');
        const badge = tab.querySelector('.bk-count');
        if (!badge) return;
        if (key === 'all') badge.textContent = total.toString();
        if (key === 'pending') badge.textContent = pending.toString();
        if (key === 'scheduled')
          badge.textContent = active.filter((b: any) => b.tab === 'scheduled').length.toString();
        if (key === 'signed')
          badge.textContent = active.filter((b: any) => b.tab === 'signed').length.toString();
        if (key === 'completed') badge.textContent = completed.toString();
      });

      const sset = (cls: string, val: any) => {
        const el = document.querySelector(cls);
        if (el) el.textContent = val;
      };
      sset('.bk-si-blue .bk-si-num', total);
      sset('.bk-si-red .bk-si-num', pending);
      sset('.bk-si-amber .bk-si-num', inProgress);
      sset('.bk-si-green .bk-si-num', completed);

      const scheduled = active.filter((b: any) => b.tab === 'scheduled' && b.visitDate);
      const reminderCard = document.querySelector('.bk-reminder-card') as HTMLElement;
      if (reminderCard) {
        if (scheduled.length > 0) {
          const nextVisit = scheduled[0];
          reminderCard.style.display = 'block';
          const titleEl = reminderCard.querySelector('.bk-reminder-prop');
          if (titleEl) titleEl.textContent = nextVisit.name;
          const dateEl = reminderCard.querySelector('.bk-reminder-date');
          if (dateEl)
            dateEl.textContent =
              nextVisit.visitDate + (nextVisit.visitSlot ? ' · ' + nextVisit.visitSlot : '');
          const agentEl = reminderCard.querySelector('.bk-reminder-agent');
          if (agentEl)
            agentEl.textContent =
              'Agent: ' + (nextVisit.agent ? nextVisit.agent.split('—')[0].trim() : 'TBD');
          const btn = reminderCard.querySelector('.reschedule-visit-btn');
          if (btn) btn.setAttribute('data-id', nextVisit.id);
        } else {
          reminderCard.style.display = 'none';
        }
      }

      const completedProperties = active.filter((b: any) => b.tab === 'completed');

      function parseIndianPrice(price: any) {
        if (!price) return 0;
        const str = String(price).trim().toLowerCase();
        const num = parseFloat(str.replace(/[^\d.]/g, ''));
        if (isNaN(num)) return 0;
        if (str.includes('cr')) return Math.round(num * 1e7);
        if (str.includes('lac') || str.includes('lakh')) return Math.round(num * 1e5);
        return Math.round(num);
      }

      const totalVal = completedProperties.reduce((acc, b) => acc + parseIndianPrice(b.price), 0);
      const tvEl = document.querySelector('.bk-tv-amount');
      if (tvEl) tvEl.textContent = '₹ ' + totalVal.toLocaleString('en-IN');
    }

    let activeTab = 'all';
    const searchInput = document.getElementById('bookings-search') as HTMLInputElement;

    document.querySelectorAll('.bk-tab').forEach((tab: any) => {
      tab.addEventListener('click', () => {
        document.querySelectorAll('.bk-tab').forEach((t) => t.classList.remove('active'));
        tab.classList.add('active');
        activeTab = tab.getAttribute('data-tab');
        applyFilters();
      });
    });

    if (searchInput) searchInput.addEventListener('input', applyFilters);

    function applyFilters() {
      const query = searchInput ? searchInput.value.toLowerCase().trim() : '';
      const cards = Array.from(document.querySelectorAll('.bk-card'));
      let visible = 0;

      cards.forEach((card: any) => {
        const title = card.querySelector('.bk-card-title')?.textContent.toLowerCase() || '';
        const tabType = card.getAttribute('data-tab');
        const tabMatch = activeTab === 'all' || tabType === activeTab;
        const searchMatch = title.includes(query);

        if (tabMatch && searchMatch) {
          card.style.display = '';
          visible++;
        } else card.style.display = 'none';
      });

      if (emptyState) emptyState.style.display = visible === 0 ? 'block' : 'none';
    }

    function rebindEvents() {
      document.querySelectorAll('.view-details-btn').forEach((btn: any) => {
        btn.addEventListener('click', (e: any) => {
          e.stopPropagation();
          const card = btn.closest('.bk-card');
          const propId = card.dataset.propId || card.dataset.id;
          let targetUrl = '/buyer-dashboard/property-details';
          if (propId && !propId.startsWith('AWB')) {
            targetUrl += `?id=${encodeURIComponent(propId)}`;
          } else {
            const name = card.dataset.name;
            if (name) targetUrl += `?title=${encodeURIComponent(name)}`;
          }
          window.location.href = targetUrl;
        });
      });
    }

    w.openModal = function (card: any) {
      if (!card) return;
      const overlay = document.getElementById('bk-modal-overlay');
      if (!overlay) return;

      const d = card.dataset;
      const b_id = d.bookingId || d.id;
      const list = typeof loadBookings === 'function' ? loadBookings() : [];
      const bData = list.find((x: any) => x.id === b_id) || d;

      const propNameEl = document.getElementById('bk-modal-prop-name');
      if (propNameEl) propNameEl.textContent = bData.name || d.name || 'Property';

      const locEl = document.getElementById('bk-modal-loc');
      if (locEl) locEl.textContent = '|| ' + (bData.loc || d.loc || '');

      const priceEl = document.getElementById('bk-modal-price');
      if (priceEl) priceEl.textContent = bData.price || d.price || '₹ 0';

      const imgEl = document.getElementById('bk-modal-img') as HTMLImageElement;
      if (imgEl) {
        const targetImg = bData.img || d.img;
        imgEl.src =
          targetImg && targetImg.includes('assets/')
            ? '/assets/' + targetImg.split('assets/')[1]
            : '/assets/images/placeholder.jpg';
      }

      const statusBadge = document.getElementById('bk-modal-status-badge');
      if (statusBadge) {
        statusBadge.textContent = bData.status || d.status || 'Status';
        statusBadge.className =
          'bk-modal-status-badge ' + (bData.tab ? 'bk-badge-' + bData.tab : 'bk-badge-signed');
      }

      const specsHTML = [
        bData.type || d.type ? `<span class="bk-chip">🛏️ ${bData.type || d.type}</span>` : '',
        bData.size || d.size ? `<span class="bk-chip">|| ${bData.size || d.size}</span>` : '',
        bData.beds || d.beds ? `<span class="bk-chip">🛏️ ${bData.beds || d.beds}</span>` : '',
        bData.baths || d.baths ? `<span class="bk-chip">|| ${bData.baths || d.baths}</span>` : '',
      ].join('');
      const specsEl = document.getElementById('bk-modal-specs');
      if (specsEl) specsEl.innerHTML = specsHTML;

      const tlEl = document.getElementById('bk-modal-timeline');
      if (tlEl) {
        const labels = ['Site-Visit', 'Visited', 'Payment', 'Complete'];
        let html = '';
        const safeTl = bData.timeline || ['done', '', '', ''];
        safeTl.slice(0, 4).forEach((state: any, i: any) => {
          html += `<div class="bk-tl-step ${state}"><span class="bk-tl-dot"></span><span class="bk-tl-label">${labels[i]}</span></div>`;
          if (i < 3)
            html += `<div class="bk-tl-line ${safeTl[i] === 'done' && safeTl[i + 1] ? 'done' : ''}"></div>`;
        });
        tlEl.innerHTML = html;
      }

      const bookBtn = document.getElementById('bk-modal-book-btn');
      if (bookBtn) {
        if (bData.tab === 'completed' || bData.tab === 'cancelled') {
          bookBtn.style.display = 'none';
        } else {
          bookBtn.style.display = 'block';
          bookBtn.onclick = () => {
            self.toastService.info('Redirecting to completion process...');
          };
        }
      }

      overlay.classList.add('show');
      overlay.style.display = 'flex';
      document.body.style.overflow = 'hidden';
    };

    w.closeModal = function closeModal() {
      const modalOverlay = document.getElementById('bk-modal-overlay');
      if (modalOverlay) {
        modalOverlay.classList.remove('show');
        modalOverlay.style.display = 'none';
      }
      document.body.style.overflow = '';
    };

    const mc1 = document.getElementById('bk-modal-close-btn');
    const mc2 = document.getElementById('bk-modal-close-btn-2');
    if (mc1) mc1.onclick = w.closeModal;
    if (mc2) mc2.onclick = w.closeModal;

    const modalOverlay = document.getElementById('bk-modal-overlay');
    if (modalOverlay)
      modalOverlay.addEventListener('click', (e) => {
        if (e.target === modalOverlay) w.closeModal();
      });
    document.addEventListener('keydown', (e) => {
      if (e.key === 'Escape') w.closeModal();
    });



    let rsCalYear: any, rsCalMonth: any;
    let rsSelectedDate: Date | null = null;
    let rsSelectedSlot: string | null = null;
    let rsCurrentlyBookedSlotObj: any = null;
    const MONTH_NAMES = [
      'January',
      'February',
      'March',
      'April',
      'May',
      'June',
      'July',
      'August',
      'September',
      'October',
      'November',
      'December',
    ];

    function parseVisitDateTime(dateStr: string, slotStr: string) {
      if (!dateStr || dateStr === 'undefined' || dateStr === 'Not Scheduled Yet') return null;
      const parts = dateStr.trim().split(' ');
      if (parts.length < 3) return null;
      const day = parseInt(parts[0]);
      const month = MONTH_NAMES.findIndex((m) => m.toLowerCase() === parts[1].toLowerCase());
      const year = parseInt(parts[2]);
      let h = 0,
        m = 0;
      if (slotStr && slotStr !== 'undefined') {
        const timeStr = slotStr.split('-')[0].trim();
        const tParts = timeStr.split(' ');
        if (tParts.length === 2) {
          const hm = tParts[0].split(':');
          h = parseInt(hm[0]);
          m = parseInt(hm[1]);
          if (tParts[1].toUpperCase() === 'PM' && h !== 12) h += 12;
          if (tParts[1].toUpperCase() === 'AM' && h === 12) h = 0;
        }
      }
      return new Date(year, month, day, h, m, 0);
    }

    function renderRescheduleCalendar(gridEl: any, monthLabel: any, prevBtn: any, nextBtn: any) {
      const labels = gridEl.querySelectorAll('.ss-cal-day-lbl');
      gridEl.innerHTML = '';
      labels.forEach((l: any) => gridEl.appendChild(l.cloneNode(true)));

      monthLabel.textContent = `${MONTH_NAMES[rsCalMonth]} ${rsCalYear}`;

      const today = new Date();
      today.setHours(0, 0, 0, 0);

      prevBtn.disabled = rsCalYear === today.getFullYear() && rsCalMonth === today.getMonth();

      const firstDay = new Date(rsCalYear, rsCalMonth, 1).getDay();
      const daysInMonth = new Date(rsCalYear, rsCalMonth + 1, 0).getDate();

      for (let i = 0; i < firstDay; i++) {
        const empty = document.createElement('div');
        empty.className = 'ss-cal-day empty';
        gridEl.appendChild(empty);
      }

      for (let d = 1; d <= daysInMonth; d++) {
        const cell = document.createElement('div');
        cell.className = 'ss-cal-day num';
        cell.textContent = d.toString();
        const cellDate = new Date(rsCalYear, rsCalMonth, d);

        if (cellDate.getTime() <= today.getTime()) {
          cell.classList.add('disabled');
        } else {
          if (rsSelectedDate && cellDate.getTime() === rsSelectedDate.getTime()) {
            cell.classList.add('active');
          }
          cell.addEventListener('click', () => {
            gridEl
              .querySelectorAll('.ss-cal-day.num')
              .forEach((c: any) => c.classList.remove('active'));
            cell.classList.add('active');
            rsSelectedDate = cellDate;
            renderRescheduleSlots();
          });
        }
        gridEl.appendChild(cell);
      }
    }

    function renderRescheduleSlots() {
      const slotsGrid = document.getElementById('rs-slots-grid');
      if (!slotsGrid) return;
      slotsGrid.innerHTML = '';

      if (!rsSelectedDate) {
        slotsGrid.innerHTML = `<div style="grid-column:1/-1; color:#64748B; font-size:0.85rem; text-align:center;">Please select a date first.</div>`;
        return;
      }

      const allSlots = [
        '09:00 AM - 10:00 AM',
        '10:00 AM - 11:00 AM',
        '11:00 AM - 12:00 PM',
        '12:00 PM - 01:00 PM',
        '02:00 PM - 03:00 PM',
        '03:00 PM - 04:00 PM',
        '04:00 PM - 05:00 PM',
        '05:00 PM - 06:00 PM',
      ];

      const rsDateStr = rsSelectedDate.toLocaleDateString('en-IN', {
        day: 'numeric',
        month: 'long',
        year: 'numeric',
      });

      allSlots.forEach((slot) => {
        const btn = document.createElement('button') as HTMLButtonElement;
        btn.className = 'ss-slot-btn';
        btn.textContent = slot;

        if (
          rsCurrentlyBookedSlotObj &&
          rsCurrentlyBookedSlotObj.date === rsDateStr &&
          rsCurrentlyBookedSlotObj.slot === slot
        ) {
          btn.disabled = true;
          btn.style.opacity = '0.4';
          btn.style.cursor = 'not-allowed';
          btn.style.textDecoration = 'line-through';
          btn.title = 'This is your currently scheduled slot';
        } else {
          if (rsSelectedSlot === slot) btn.classList.add('active');
          btn.addEventListener('click', () => {
            slotsGrid
              .querySelectorAll('.ss-slot-btn')
              .forEach((b: any) => b.classList.remove('active'));
            btn.classList.add('active');
            rsSelectedSlot = slot;
          });
        }
        slotsGrid.appendChild(btn);
      });
    }

    w.openSiteVisitModal = function (id: string) {
      const list = loadBookings();
      const b = list.find((x: any) => x.id === id);
      if (!b) return;
      const propNameEl = document.getElementById('sv-modal-prop-name');
      if (propNameEl) propNameEl.textContent = b.name;

      let dateTimeText = 'Not Scheduled Yet';
      if (b.visitDate && b.visitDate !== 'undefined') {
        dateTimeText = b.visitDate;
        if (b.visitSlot && b.visitSlot !== 'undefined') {
          dateTimeText += ' at ' + b.visitSlot;
        }
        rsCurrentlyBookedSlotObj = { date: b.visitDate, slot: b.visitSlot };
      } else {
        rsCurrentlyBookedSlotObj = null;
      }
      const dtEl = document.getElementById('sv-modal-date');
      if (dtEl) dtEl.textContent = dateTimeText;
      const agentEl = document.getElementById('sv-modal-agent');
      if (agentEl) agentEl.textContent = b.agent || 'Not Assigned';
      const locEl = document.getElementById('sv-modal-loc');
      if (locEl) locEl.textContent = b.loc;

      const rSection = document.getElementById('reschedule-section');
      if (rSection) {
        rSection.style.display = 'none';
        rSection.innerHTML = '';
      }

      const svResBtn = document.getElementById('sv-reschedule-btn');
      if (svResBtn) {
        svResBtn.style.display = 'inline-block';
        const svWarn = document.getElementById('sv-4hour-warn');
        if (svWarn) svWarn.remove();

        const visitDt = parseVisitDateTime(b.visitDate, b.visitSlot);
        if (visitDt) {
          const today = new Date();
          today.setHours(0, 0, 0, 0);
          const visitDateOnly = new Date(visitDt);
          visitDateOnly.setHours(0, 0, 0, 0);

          if (today.getTime() >= visitDateOnly.getTime()) {
            svResBtn.style.display = 'none';
            const warn = document.createElement('div');
            warn.id = 'sv-4hour-warn';
            warn.style.cssText =
              'margin-top: 15px; color: #DC2626; font-size: 0.85rem; font-weight: 600; text-align: center; background: #FEF2F2; padding: 10px; border-radius: 8px; border: 1px solid #FECACA;';
            warn.innerHTML =
              '🚫 Rescheduling is only allowed before the scheduled visit date.';
            if (svResBtn.parentNode) svResBtn.parentNode.insertBefore(warn, svResBtn);
          }
        }
      }

      const svOverlay = document.getElementById('site-visit-modal-overlay');
      if (svOverlay) {
        svOverlay.style.display = 'flex';
        svOverlay.classList.add('show');
        svOverlay.dataset['currentId'] = id;
      }
    };

    document.querySelectorAll('.reschedule-visit-btn').forEach((btn: any) => {
      btn.addEventListener('click', () => {
        const id =
          btn.getAttribute('data-id') || btn.closest('.bk-card')?.getAttribute('data-booking-id');
        if (id) w.openSiteVisitModal(id);
      });
    });

    const newBookingBtn = document.getElementById('new-booking-btn');
    if (newBookingBtn) {
      newBookingBtn.addEventListener('click', () => {
        window.location.href = '/buyer-dashboard/properties';
      });
    }

    const svRescheduleBtn = document.getElementById('sv-reschedule-btn');
    if (svRescheduleBtn) {
      svRescheduleBtn.addEventListener('click', () => {
        const rSection = document.getElementById('reschedule-section');
        if (rSection) {
          rSection.style.display = 'block';

          if (!document.getElementById('ss-calendar-grid')) {
            rSection.innerHTML = `
              <h4 style="margin-top:0; color:#1E3A8A; margin-bottom:12px;">Reschedule Visit</h4>
              <div class="ss-selector-grid" style="margin-bottom: 20px;">
                <div class="ss-card ss-calendar-card" style="padding:16px;">
                  <div class="ss-calendar-header">
                    <button class="ss-cal-nav" id="ss-cal-prev">&#8592;</button>
                    <div class="ss-card-title" id="ss-cal-month-label" style="margin:0; border:none; padding:0;"></div>
                    <button class="ss-cal-nav" id="ss-cal-next">&#8594;</button>
                  </div>
                  <div class="ss-calendar-grid" id="ss-calendar-grid">
                    <div class="ss-cal-day-lbl">S</div>
                    <div class="ss-cal-day-lbl">M</div>
                    <div class="ss-cal-day-lbl">T</div>
                    <div class="ss-cal-day-lbl">W</div>
                    <div class="ss-cal-day-lbl">T</div>
                    <div class="ss-cal-day-lbl">F</div>
                    <div class="ss-cal-day-lbl">S</div>
                  </div>
                </div>
                <div class="ss-card ss-slots-card" style="padding:16px;">
                  <div class="ss-card-title" style="margin-bottom:12px;">Select Time Slot</div>
                  <div class="ss-slots-grid" id="rs-slots-grid"></div>
                </div>
              </div>
              <button id="reschedule-submit-btn-custom" class="bk-btn-primary" style="width:100%; justify-content:center;">Confirm Reschedule</button>
            `;

            const now = new Date();
            rsCalMonth = now.getMonth();
            rsCalYear = now.getFullYear();
            rsSelectedDate = null;
            rsSelectedSlot = null;

            const gridEl = document.getElementById('ss-calendar-grid');
            const monthLbl = document.getElementById('ss-cal-month-label');
            const prevB = document.getElementById('ss-cal-prev');
            const nextB = document.getElementById('ss-cal-next');

            if (prevB)
              prevB.addEventListener('click', () => {
                if (rsCalMonth === 0) {
                  rsCalMonth = 11;
                  rsCalYear--;
                } else {
                  rsCalMonth--;
                }
                renderRescheduleCalendar(gridEl, monthLbl, prevB, nextB);
                renderRescheduleSlots();
              });
            if (nextB)
              nextB.addEventListener('click', () => {
                if (rsCalMonth === 11) {
                  rsCalMonth = 0;
                  rsCalYear++;
                } else {
                  rsCalMonth++;
                }
                renderRescheduleCalendar(gridEl, monthLbl, prevB, nextB);
                renderRescheduleSlots();
              });

            renderRescheduleCalendar(gridEl, monthLbl, prevB, nextB);
            renderRescheduleSlots();

            const rsubBtn = document.getElementById('reschedule-submit-btn-custom');
            if (rsubBtn)
              rsubBtn.addEventListener('click', () => {
                if (!rsSelectedDate || !rsSelectedSlot) {
                  self.toastService.error('Please select a date and a time slot.');
                  return;
                }
                const svOverlay = document.getElementById('site-visit-modal-overlay');
                const id = svOverlay ? svOverlay.dataset['currentId'] : null;
                if (id) {
                  // Combine date and slot
                  const timeParts = rsSelectedSlot.split(' - ')[0].split(':');
                  let hour = parseInt(timeParts[0], 10);
                  const minute = parseInt(timeParts[1].substring(0, 2), 10);
                  const ampm = timeParts[1].substring(3, 5);
                  if (ampm === 'PM' && hour !== 12) hour += 12;
                  if (ampm === 'AM' && hour === 12) hour = 0;

                  const scheduledDate = new Date(
                    Date.UTC(
                      rsSelectedDate.getFullYear(),
                      rsSelectedDate.getMonth(),
                      rsSelectedDate.getDate(),
                      hour,
                      minute,
                      0,
                      0,
                    ),
                  ).toISOString();

                  rsubBtn.textContent = 'Rescheduling...';
                  self.bookingService.rescheduleBooking(parseInt(id, 10), scheduledDate).subscribe({
                    next: () => {
                      if (rsSelectedDate) {
                        const dStr = rsSelectedDate.toLocaleDateString('en-IN', {
                          day: 'numeric',
                          month: 'long',
                          year: 'numeric',
                        });
                        const svDateEl = document.getElementById('sv-modal-date');
                        if (svDateEl) svDateEl.textContent = dStr + ' at ' + rsSelectedSlot;
                      }
                      const rSection = document.getElementById('reschedule-section');
                      if (rSection) rSection.style.display = 'none';
                      self.toastService.success('Visit rescheduled successfully!');
                      renderBookings();
                      rsubBtn.textContent = 'Confirm Reschedule';
                    },
                    error: (err: any) => {
                      console.error(err);
                      self.toastService.error('Failed to reschedule.');
                      rsubBtn.textContent = 'Confirm Reschedule';
                    },
                  });
                }
              });
          }
        }
      });
    }

    const svCloseBtn = document.getElementById('sv-close-btn');
    if (svCloseBtn) {
      svCloseBtn.addEventListener('click', () => {
        const svOverlay = document.getElementById('site-visit-modal-overlay');
        if (svOverlay) {
          svOverlay.classList.remove('show');
          svOverlay.style.display = 'none';
        }
      });
    }

    const rescheduleSubmitBtn = document.getElementById('reschedule-submit-btn');
    if (rescheduleSubmitBtn) {
      rescheduleSubmitBtn.addEventListener('click', () => {
        const rsDt = document.getElementById('reschedule-datetime') as HTMLInputElement;
        const newSlot = rsDt ? rsDt.value : null;
        if (!newSlot) {
          alert('Please select a valid date and time.');
          return;
        }
        const svOverlay = document.getElementById('site-visit-modal-overlay');
        const id = svOverlay ? svOverlay.dataset['currentId'] : null;
        if (id) {
          const d = new Date(newSlot);
          self.bookingService.rescheduleBooking(parseInt(id, 10), d.toISOString()).subscribe({
            next: () => {
              self.toastService.success('Reschedule request logged. Agent will confirm shortly.');
              renderBookings();
              if (svOverlay) svOverlay.style.display = 'none';
            },
            error: (err: any) => {
              console.error(err);
              self.toastService.error('Failed to reschedule.');
            },
          });
        }
      });
    }

    const phAction = document.getElementById('payment-history-action');
    const phOverlay = document.getElementById('payment-history-modal-overlay');
    const phCloseBtn = document.getElementById('ph-close-btn');
    const phTbody = document.getElementById('ph-modal-tbody');

    function openPaymentHistory() {
      const list = loadBookings();
      if (!phTbody) return;
      const paymentRows = list.filter(
        (b: any) => b.paid || b.cancelled || b.tab === 'completed' || b.tab === 'registered',
      );
      if (paymentRows.length === 0) {
        phTbody.innerHTML = `<tr><td colspan="4" style="padding:24px;text-align:center;color:#94A3B8;">No payment records found yet.</td></tr>`;
      } else {
        phTbody.innerHTML = paymentRows
          .map((b: any) => {
            const priceRaw = (b.price || b.totalPrice || '').toString().replace(/[^\d]/g, '');
            const priceNum = parseInt(priceRaw, 10) || 0;
            const displayAmt = priceNum
              ? '₹ ' + (priceNum / 100).toLocaleString('en-IN') + ' Cr'
              : b.price || 'N/A';
            let statusColor = '#10B981';
            let statusBg = '#D1FAE5';
            let statusText = 'Paid';
            if (b.cancelled) {
              statusColor = '#EF4444';
              statusBg = '#FEE2E2';
              statusText = 'Cancelled';
            } else if (b.tab === 'completed' || b.tab === 'registered') {
              statusColor = '#10B981';
              statusBg = '#D1FAE5';
              statusText = 'Completed';
            } else if (b.paid) {
              statusColor = '#3B82F6';
              statusBg = '#DBEAFE';
              statusText = 'Paid';
            }
            return `<tr style="border-bottom:1px solid #E2E8F0;">
            <td style="padding:14px 16px;">
              <div style="font-weight:700;color:#1E293B;">${b.name}</div>
              <div style="font-size:0.78rem;color:#64748B;">${b.loc || ''}</div>
            </td>
            <td style="padding:14px 16px;font-size:0.82rem;color:#475569;">${b.id}</td>
            <td style="padding:14px 16px;">
              <span style="background:${statusBg};color:${statusColor};padding:4px 10px;border-radius:20px;font-size:0.78rem;font-weight:700;">
                ${statusText}
              </span>
            </td>
            <td style="padding:14px 16px;text-align:right;font-weight:700;color:#D97706;">${displayAmt}</td>
          </tr>`;
          })
          .join('');
      }
      if (phOverlay) {
        phOverlay.style.display = 'flex';
      }
    }

    if (phAction) phAction.addEventListener('click', openPaymentHistory);
    if (phCloseBtn)
      phCloseBtn.addEventListener('click', () => {
        if (phOverlay) phOverlay.style.display = 'none';
      });
    if (phOverlay)
      phOverlay.addEventListener('click', (e) => {
        if (e.target === phOverlay) phOverlay.style.display = 'none';
      });

    let currentPostVisitBookingId: string | null = null;
    const postVisitModal = document.getElementById('post-visit-modal-overlay');
    const postVisitClose = document.getElementById('post-visit-close');
    const btnInterested = document.getElementById('btn-interested');
    const btnNotInterested = document.getElementById('btn-not-interested');

    document.addEventListener('click', (e: any) => {
      const markBtn = e.target.closest('.mark-visit-complete-btn');
      if (markBtn) {
        currentPostVisitBookingId = markBtn.getAttribute('data-id');
        if (!currentPostVisitBookingId) return;

        markBtn.textContent = 'Processing...';
        markBtn.disabled = true;
        self.bookingService.markVisited(parseInt(currentPostVisitBookingId, 10)).subscribe({
          next: () => {
            self.toastService.success('Site visit completed!');
            renderBookings();
            if (postVisitModal) {
              postVisitModal.style.display = 'flex';
              postVisitModal.classList.add('show');
            }
          },
          error: (err: any) => {
            console.error(err);
            self.toastService.error('Failed to mark site visit completed.');
            markBtn.textContent = 'Mark Visited';
            markBtn.disabled = false;
          }
        });
      }

      const intBtn = e.target.closest('.btn-interested-direct-btn');
      if (intBtn) {
        currentPostVisitBookingId = intBtn.getAttribute('data-id');
        if (currentPostVisitBookingId) {
          const btnInt = document.getElementById('btn-interested');
          if (btnInt) btnInt.click();
        }
      }

      const passBtn = e.target.closest('.pass-prop-btn');
      if (passBtn) {
        if (
          !confirm(
            'Are you sure you want to pass on this property? This will cancel your current booking and make the property available to others.',
          )
        ) {
          return;
        }

        const id = passBtn.getAttribute('data-id');
        if (!id) return;

        self.bookingService.cancelBooking(parseInt(id, 10)).subscribe({
          next: () => {
            self.toastService.success('Passed on property. Booking cancelled.');
            renderBookings();
          },
          error: (err: any) => {
            console.error(err);
            self.toastService.error('Failed to pass on property.');
          },
        });
      }
    });

    if (postVisitClose) {
      postVisitClose.addEventListener('click', () => {
        if (postVisitModal) {
          postVisitModal.classList.remove('show');
          setTimeout(() => (postVisitModal.style.display = 'none'), 300);
        }
      });
    }

    if (btnInterested) {
      btnInterested.addEventListener('click', () => {
        if (!currentPostVisitBookingId) return;

        btnInterested.textContent = 'Saving...';
        self.bookingService.recordInterest(parseInt(currentPostVisitBookingId, 10), 'Interested').subscribe({
          next: () => {
            self.toastService.success('Interest recorded! Redirecting to payments...');
            
            const list = loadBookings();
            const bookingId = currentPostVisitBookingId;
            if (!bookingId) return;
            const b = list.find((x: any) => x.id === bookingId);
            
            if (postVisitModal) {
              postVisitModal.classList.remove('show');
              postVisitModal.style.display = 'none';
            }
            
            let queryParams = `?id=${encodeURIComponent(bookingId)}`;
            if (b) {
              queryParams += `&name=${encodeURIComponent(b.name)}&loc=${encodeURIComponent(b.loc)}&price=${encodeURIComponent(b.price)}&img=${encodeURIComponent(b.img || '')}`;
            }
            btnInterested.textContent = 'Interested';
            window.location.href = `/buyer-dashboard/payment-management${queryParams}`;
          },
          error: (err: any) => {
            console.error(err);
            self.toastService.error('Failed to record interest.');
            btnInterested.textContent = 'Interested';
          }
        });
      });
    }

    if (btnNotInterested) {
      btnNotInterested.addEventListener('click', () => {
        if (!currentPostVisitBookingId) return;

        btnNotInterested.textContent = 'Saving...';
        self.bookingService.recordInterest(parseInt(currentPostVisitBookingId, 10), 'NotInterested').subscribe({
          next: () => {
            self.toastService.info('Not Interested recorded. Booking cancelled and property released.');
            renderBookings();
            if (postVisitModal) {
              postVisitModal.classList.remove('show');
              postVisitModal.style.display = 'none';
            }
            btnNotInterested.textContent = 'Not Interested';
          },
          error: (err: any) => {
            console.error(err);
            self.toastService.error('Failed to record Not Interested response.');
            btnNotInterested.textContent = 'Not Interested';
          }
        });
      });
    }

    document.addEventListener('click', (e: any) => {
      const siteVisitBtn =
        e.target.closest('.bk-btn-status.scheduled') ||
        e.target.closest("[data-action='site-visit']");
      if (siteVisitBtn) {
        e.stopPropagation();
        const id =
          siteVisitBtn.closest('.bk-card')?.getAttribute('data-id') ||
          siteVisitBtn.getAttribute('data-id');
        if (id) w.openSiteVisitModal(id);
        return;
      }

      const cancelBtn = e.target.closest('.cancel-booking-btn');

      if (!cancelBtn) return;

      e.stopPropagation();
      try {
        const paid = cancelBtn.getAttribute('data-paid') === 'true';
        const name = cancelBtn.getAttribute('data-name') || 'Property';
        const bookingId = cancelBtn.getAttribute('data-id');

        w.currentCancelId = bookingId;

        const propNameEl = document.getElementById('cancel-prop-name');
        if (propNameEl) propNameEl.textContent = name;

        const refundBox = document.getElementById('cancel-refund-box');
        const freeBox = document.getElementById('cancel-free-box');

        const booking = allBookings.find(b => b.id === bookingId);
        const paidDate = booking?.createdAtRaw ? new Date(booking.createdAtRaw) : new Date();
        const daysSincePayment = Math.floor((Date.now() - paidDate.getTime()) / (1000 * 60 * 60 * 24));

        if (paid) {
          if (daysSincePayment > 30) {
            self.toastService.error('Cannot cancel booking more than 30 days after payment.');
            return;
          }

          const actualPaid = 10000; // Fixed token advance amount
          let cancellationFee = 0;
          let netRefund = 0;

          if (daysSincePayment <= 7) {
            cancellationFee = 1000; // 10% fee
            netRefund = 9000;
          } else {
            cancellationFee = 10000; // 100% fee
            netRefund = 0;
          }

          if (refundBox) refundBox.style.display = 'block';
          if (freeBox) freeBox.style.display = 'none';
          const rPaid = document.getElementById('refund-paid-amount');
          if (rPaid) rPaid.textContent = '₹ ' + actualPaid.toLocaleString('en-IN');
          const rFee = document.getElementById('refund-fee-amount');
          if (rFee) rFee.textContent = '₹ ' + cancellationFee.toLocaleString('en-IN');
          const rNet = document.getElementById('refund-net-amount');
          if (rNet) rNet.textContent = '₹ ' + netRefund.toLocaleString('en-IN');
        } else {
          if (refundBox) refundBox.style.display = 'none';
          if (freeBox) freeBox.style.display = 'flex';
        }

        const confirmCheck = document.getElementById('cancel-confirm-check') as HTMLInputElement;
        const confirmBtn = document.getElementById('cancel-submit-btn') as HTMLButtonElement;
        if (confirmCheck) confirmCheck.checked = false;
        if (confirmBtn) confirmBtn.disabled = true;

        const modal = document.getElementById('cancel-modal-overlay');
        if (modal) {
          modal.classList.add('show');
          modal.style.display = 'flex';
        } else {
          console.error('Cancel modal overlay not found!');
        }
      } catch (err) {
        console.error('Error opening cancel modal:', err);
      }
    });

    function closeCancel() {
      const cancelModal = document.getElementById('cancel-modal-overlay');
      if (cancelModal) {
        cancelModal.classList.remove('show');
        cancelModal.style.display = 'none';
      }
      w.currentCancelId = null;
    }

    const cxCancel = document.getElementById('cancel-modal-close');
    if (cxCancel) cxCancel.onclick = closeCancel;

    document.addEventListener('click', (e) => {
      const cancelModal = document.getElementById('cancel-modal-overlay');
      if (cancelModal && e.target === cancelModal) {
        closeCancel();
      }
    });

    const cfmCancel = document.getElementById('cancel-submit-btn') as HTMLButtonElement;
    const checkCancel = document.getElementById('cancel-confirm-check') as HTMLInputElement;

    if (checkCancel && cfmCancel) {
      checkCancel.addEventListener('change', () => {
        cfmCancel.disabled = !checkCancel.checked;
      });
    }

    if (cfmCancel) {
      cfmCancel.onclick = () => {
        if (!w.currentCancelId) return;

        const cancelledId = w.currentCancelId;
        const list = loadBookings();
        const cancelledBooking = list.find((b: any) => b.id === cancelledId);

        cancelBookingById(cancelledId);
        closeCancel();
        renderBookings();

        // Show success modal
        const successOverlay = document.getElementById('cancel-success-modal-overlay');
        const successPropName = document.getElementById('cancel-success-prop-name');
        if (successPropName && cancelledBooking) {
          const paid = cancelledBooking.paid;
          const paidDate = cancelledBooking.createdAtRaw ? new Date(cancelledBooking.createdAtRaw) : new Date();
          const daysSincePayment = Math.floor((Date.now() - paidDate.getTime()) / (1000 * 60 * 60 * 24));
          
          let refundText = "";
          if (paid) {
            if (daysSincePayment <= 7) {
              refundText = "A 10% fee (₹1,000) was charged. Your refund of <strong>₹9,000</strong> will be processed within 5–7 working days.";
            } else {
              refundText = "A 100% fee (₹10,000) was charged. No refund is applicable.";
            }
          } else {
            refundText = "<strong>No charges applied.</strong>";
          }

          successPropName.innerHTML = `Your booking for <strong>${cancelledBooking.name}</strong> has been cancelled. ${refundText}`;
        }
        if (successOverlay) {
          successOverlay.style.display = 'flex';
          successOverlay.classList.add('show');
        }
      };
    }

    const cancelSuccessOverlay = document.getElementById('cancel-success-modal-overlay');
    const cancelSuccessCloseBtn = document.getElementById('cancel-success-close-btn');
    if (cancelSuccessCloseBtn) {
      cancelSuccessCloseBtn.addEventListener('click', () => {
        if (cancelSuccessOverlay) {
          cancelSuccessOverlay.classList.remove('show');
          cancelSuccessOverlay.style.display = 'none';
        }
      });
    }
    if (cancelSuccessOverlay) {
      cancelSuccessOverlay.addEventListener('click', (e) => {
        if (e.target === cancelSuccessOverlay) {
          cancelSuccessOverlay.classList.remove('show');
          cancelSuccessOverlay.style.display = 'none';
        }
      });
    }

    // Read query params for Razorpay callback redirect
    this.route.queryParams.subscribe(params => {
      const plinkId = params['razorpay_payment_link_id'];
      const status = params['razorpay_payment_link_status'];
      if (plinkId && status === 'paid') {
        self.toastService.info('Verifying payment status with Razorpay...');
        this.paymentService.verifyPayment(plinkId).subscribe({
          next: (res: any) => {
            self.toastService.success('Payment successful! Welcome aboard.');
            
            // Broadcast success to other tabs (such as the original payment tab)
            const targetBookingId = res?.data?.bookingId || '';
            localStorage.setItem('payment_completed_sync', JSON.stringify({ bookingId: targetBookingId, time: Date.now() }));

            // Clean up query params from URL so refresh doesn't trigger verification again
            this.router.navigate([], {
              relativeTo: this.route,
              queryParams: {
                razorpay_payment_link_id: null,
                razorpay_payment_link_status: null,
                razorpay_payment_id: null,
                razorpay_signature: null
              },
              queryParamsHandling: 'merge'
            });
            renderBookings();
          },
          error: (err) => {
            console.error('Payment verification failed', err);
            self.toastService.error('Verification failed. If payment went through, contact admin.');
            renderBookings();
          }
        });
      } else {
        renderBookings();
      }
    });

    // Cross-tab synchronization listener: refreshes list if payment verified in new tab
    window.addEventListener('storage', (event) => {
      if (event.key === 'payment_completed_sync') {
        renderBookings();
      }
    });

    this.timerInterval = setInterval(() => {
      document.querySelectorAll('.countdown-timer').forEach((el: any) => {
        const id = el.getAttribute('data-id');
        if (!id) return;
        
        let startTime = localStorage.getItem(`visit_time_${id}`);
        if (!startTime) {
           startTime = Date.now().toString();
           localStorage.setItem(`visit_time_${id}`, startTime);
        }
        
        const elapsed = Date.now() - parseInt(startTime, 10);
        const remaining = (15 * 60 * 1000) - elapsed; // 15 minutes
        
        if (remaining <= 0) {
           el.textContent = 'Expired';
           if (!el.hasAttribute('data-expired')) {
              el.setAttribute('data-expired', 'true');
              self.bookingService.cancelBooking(parseInt(id, 10)).subscribe({
                 next: () => {
                    self.toastService.error('15-minute token advance window expired. Booking cancelled.');
                    localStorage.removeItem(`visit_time_${id}`);
                    renderBookings();
                 }
              });
           }
        } else {
           const mins = Math.floor(remaining / 60000).toString().padStart(2, '0');
           const secs = Math.floor((remaining % 60000) / 1000).toString().padStart(2, '0');
           el.textContent = `⏳ Expires in ${mins}:${secs}`;
        }
      });
    }, 1000);
  }

  ngOnDestroy() {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }
}


