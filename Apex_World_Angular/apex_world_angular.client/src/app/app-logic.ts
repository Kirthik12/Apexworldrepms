// @ts-nocheck
import { environment } from '../environments/environment';

function initAppJs() {
  console.log("Apex World Premium Landing Page App Initialized.");

  // Global variables
  let testIndex = 0;
  let testAutoInterval = null;
  let autoSlideInterval = null;
  let currentSlideIndex = 0;
  const slides = [
    "assets/images/hero.png",
    "assets/images/slide_urban_condo.png",
    "assets/images/slide_lake_villa.png",
    "assets/images/slide_interior_lounge.png"
  ];

  // Global event interceptor for ripple buttons
  document.body.addEventListener("click", (e) => {
    const rippleBtn = e.target.closest(".ripple-btn");
    if (rippleBtn) {
      createRipple(e, rippleBtn);
    }
    
    // Wishlist / save action toggles
    const btn = e.target.closest(".quick-action-btn");
    if (btn && btn.getAttribute("aria-label") === "Save Property") {
      e.stopPropagation();
      e.preventDefault();
      if (btn.textContent.trim() === "🤍") {
        btn.textContent = "❤️";
        btn.style.transform = "scale(1.25)";
        btn.style.boxShadow = "var(--shadow-glow)";
        setTimeout(() => {
          btn.style.transform = "";
          btn.style.boxShadow = "";
        }, 300);
      } else {
        btn.textContent = "🤍";
      }
    }
  });

  function createRipple(event, button) {
    const circle = document.createElement("span");
    const diameter = Math.max(button.clientWidth, button.clientHeight);
    const radius = diameter / 2;

    circle.style.width = circle.style.height = `${diameter}px`;
    circle.style.left = `${event.clientX - button.getBoundingClientRect().left - radius}px`;
    circle.style.top = `${event.clientY - button.getBoundingClientRect().top - radius}px`;
    circle.classList.add("ripple-effect");

    const ripple = button.getElementsByClassName("ripple-effect")[0];
    if (ripple) {
      ripple.remove();
    }

    button.appendChild(circle);
    setTimeout(() => circle.remove(), 600);
  }

  // Navigation smooth active indicators
  const navLinks = document.querySelectorAll("nav a");
  navLinks.forEach(link => {
    link.addEventListener("click", () => {
      navLinks.forEach(l => l.classList.remove("active"));
      link.classList.add("active");
    });
  });

  // Enquiry Form Submission Interactivity
  const form = document.querySelector(".enquiry-form");
  if (form) {
    // Inline Error Helpers
    function showInlineError(inputId, message) {
      const inputEl = document.getElementById(inputId);
      if (!inputEl) return;
      clearInlineError(inputId);
      inputEl.style.borderColor = "#ef4444";
      const parent = inputEl.closest(".floating-group") || inputEl.parentElement;
      if (parent) {
        const errorSpan = document.createElement("span");
        errorSpan.className = "inline-error";
        errorSpan.style.color = "#ef4444";
        errorSpan.style.fontSize = "0.78rem";
        errorSpan.style.marginTop = "4px";
        errorSpan.style.display = "block";
        errorSpan.style.fontWeight = "500";
        errorSpan.textContent = message;
        parent.appendChild(errorSpan);
      }
    }

    function clearInlineError(inputId) {
      const inputEl = document.getElementById(inputId);
      if (!inputEl) return;
      inputEl.style.borderColor = "";
      const parent = inputEl.closest(".floating-group") || inputEl.parentElement;
      if (parent) {
        const existing = parent.querySelector(".inline-error");
        if (existing) existing.remove();
      }
    }

    form.addEventListener("submit", (e) => {
      e.preventDefault();
      
      const name = document.getElementById("enq-name").value.trim();
      const phone = document.getElementById("enq-phone").value.trim();
      const email = document.getElementById("enq-email").value.trim();
      const notRobot = document.getElementById("not-robot").checked;
      
      // Reset errors
      clearInlineError("enq-name");
      clearInlineError("enq-phone");
      clearInlineError("enq-email");
      clearInlineError("not-robot");

      let isValid = true;
      
      // Name validation: greater than 4 characters
      if (name.length <= 4) {
        showInlineError("enq-name", "Full Name must be greater than 4 characters.");
        isValid = false;
      }
      
      // Email validation: must contain '@', '.com' and be a valid format
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!email.includes("@") || !email.includes(".com") || !emailRegex.test(email)) {
        showInlineError("enq-email", "Must contain '@' and '.com' in a valid email format.");
        isValid = false;
      }
      
      // Phone validation: exactly 10 digits and starting with 6, 7, 8, or 9
      const phoneRegex = /^[6-9]\d{9}$/;
      if (!phoneRegex.test(phone)) {
        showInlineError("enq-phone", "Must have exactly 10 digits starting with 6, 7, 8, or 9.");
        isValid = false;
      }
      
      if (!notRobot) {
        showInlineError("not-robot", "Please confirm you are not a robot.");
        isValid = false;
      }

      if (!isValid) return;

      const submitBtn = form.querySelector("button[type='submit']");
      const originalText = submitBtn.textContent;
      submitBtn.disabled = true;
      submitBtn.innerHTML = '<span class="btn-loader"></span> Submitting...';

      const payload = {
        buyerName: name,
        email: email,
        phone: phone,
        message: "General Enquiry from Landing Page"
      };

      function showNotificationToast(msg: string, isSuccess: boolean) {
        if ((window as any).toastService) {
          if (isSuccess) {
            (window as any).toastService.success(msg);
          } else {
            (window as any).toastService.error(msg);
          }
        } else {
          alert(msg);
        }
      }

      fetch(`${environment.apiUrl}/Enquiry`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
      })
      .then(res => {
        if (!res.ok) throw new Error("Network response was not ok");
        return res.json();
      })
      .then(data => {
        submitBtn.innerHTML = '<span class="success-checkmark">✓</span> Sent!';
        setTimeout(() => {
          showNotificationToast("Success! Your enquiry has been submitted. Our agents will contact you shortly.", true);
          form.reset();
          submitBtn.disabled = false;
          submitBtn.textContent = originalText;
        }, 500);
      })
      .catch(error => {
        console.error("Error submitting enquiry:", error);
        showNotificationToast("There was an error submitting your enquiry. Please try again later.", false);
        submitBtn.disabled = false;
        submitBtn.textContent = originalText;
      });
    });

    // Realtime error clearing
    document.getElementById("enq-name")?.addEventListener("input", () => clearInlineError("enq-name"));
    document.getElementById("enq-phone")?.addEventListener("input", () => clearInlineError("enq-phone"));
    document.getElementById("enq-email")?.addEventListener("input", () => clearInlineError("enq-email"));
    document.getElementById("not-robot")?.addEventListener("change", () => clearInlineError("not-robot"));
  }

  // Dynamic Render Featured Properties
  function renderFeaturedProperties(filterQuery = "") {
    const grid = document.getElementById("featured-properties-grid");
    if (!grid) return;
    
    let sourceProperties = [];
    
    try {
      const raw = localStorage.getItem("landing_content");
      if(raw) {
        const content = JSON.parse(raw);
        if(content.featured && content.featured.length > 0) {
          sourceProperties = content.featured;
        }
      }
    } catch(e) {}

    // Fallback to DB if not set in CMS
    if (sourceProperties.length === 0 && typeof DB !== 'undefined') {
      let dbProps = DB.get("properties");
      // Explicitly enforce Green Meadows Villa on the landing page
      const greenMeadowsIdx = dbProps.findIndex(p => p.title === 'Green Meadows Villa');
      if (greenMeadowsIdx !== -1) {
        const greenMeadows = dbProps.splice(greenMeadowsIdx, 1)[0];
        dbProps.unshift(greenMeadows);
      }
      // Remove Sunrise Studio (PROP-003) from landing page
      sourceProperties = dbProps.filter(p => p.id !== 'PROP-003');
    }

    let properties = [...sourceProperties];

    // Automatic search filter
    if (filterQuery.trim() !== "") {
      const q = filterQuery.toLowerCase();
      properties = properties.filter(prop => 
        prop.title.toLowerCase().includes(q) || 
        prop.location.toLowerCase().includes(q) || 
        (prop.category && prop.category.toLowerCase().includes(q)) ||
        (prop.tag && prop.tag.toLowerCase().includes(q))
      );
    }

    properties = properties.slice(0, 3);
    grid.innerHTML = "";
    
    if (properties.length === 0) {
      grid.innerHTML = `<div style="grid-column: 1 / -1; text-align: center; padding: 40px; color: var(--text-muted); font-weight: 600;">No properties match your search criteria.</div>`;
      return;
    }
    
    properties.forEach(prop => {
      const card = document.createElement("div");
      card.className = "property-card";
      card.setAttribute("tabindex", "0");
      
      const imagePath = prop.img ? prop.img.replace("../../", "") : (prop.image ? prop.image.replace("../../", "") : "");
      const propTag = prop.tag || "SALE";
      const propBeds = prop.beds || "3";
      const propBaths = prop.baths || "3";
      const propSqft = prop.sqft || "1200";
      
      card.innerHTML = `
        <div class="property-image-wrapper">
          <img src="${imagePath}" alt="${prop.title}">
          <div class="property-tag">${propTag}</div>
          <div class="property-gallery-indicator">📷 1/4</div>
          <div class="property-quick-actions">
            <button class="quick-action-btn" aria-label="Save Property">🤍</button>
            <button class="quick-action-btn" aria-label="Share Property">🔗</button>
            <button class="quick-action-btn" aria-label="Contact Agent">📞</button>
          </div>
        </div>
        <div class="property-content">
          <div class="property-specs">
            <span>🛏️ ${propBeds} beds</span> • 
            <span>🛁 ${propBaths} baths</span> • 
            <span>📐 ${propSqft} sqft</span>
          </div>
          <h3 class="property-title">${prop.title}</h3>
          <div class="property-rating">⭐⭐⭐⭐ 5.0 (12 reviews)</div>
          <p class="property-location">
            <svg width="14" height="14" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z"></path></svg> 
            ${prop.location}
          </p>
          <div class="property-footer">
            <div class="property-price">₹ ${prop.price}</div>
            <a href="/login" class="btn btn-outline btn-sm ripple-btn">View Details</a>
          </div>
        </div>`;
      grid.appendChild(card);
    });
  }

  renderFeaturedProperties();

  // Search input listeners
  const searchInput = document.getElementById("hero-search-input");
  const searchBtn = document.getElementById("hero-search-btn");

  if (searchInput) {
    searchInput.addEventListener("input", (e) => {
      renderFeaturedProperties(e.target.value);
    });
  }

  if (searchBtn) {
    searchBtn.addEventListener("click", (e) => {
      e.preventDefault();
      e.stopPropagation();
      const query = searchInput ? searchInput.value : "";
      renderFeaturedProperties(query);
      
      const section = document.getElementById("featured-properties");
      if (section) {
        section.scrollIntoView({ behavior: "smooth", block: "start" });
      }
    });
  }

  // Initialize Hero slider
  function initializeHeroSlider() {
    const heroCard = document.querySelector(".hero-card");
    if (!heroCard) return;

    function updateSlide(index) {
      currentSlideIndex = index;
      heroCard.style.backgroundImage = "linear-gradient(rgba(15, 27, 53, 0.45), rgba(15, 27, 53, 0.2)), url('" + slides[currentSlideIndex] + "')";
      heroCard.style.backgroundSize = "cover";
      heroCard.style.backgroundPosition = "center";
      heroCard.style.backgroundRepeat = "no-repeat";
      
      const dotsList = document.querySelectorAll(".hero-dots .hero-dot");
      dotsList.forEach((dot, idx) => {
        if (idx === currentSlideIndex) {
          dot.classList.add("active");
        } else {
          dot.classList.remove("active");
        }
      });
    }

    function startAutoSlide() {
      stopAutoSlide();
      autoSlideInterval = setInterval(() => {
        const nextIndex = (currentSlideIndex + 1) % slides.length;
        updateSlide(nextIndex);
      }, 4500);
    }

    function stopAutoSlide() {
      if (autoSlideInterval) clearInterval(autoSlideInterval);
    }

    updateSlide(0);
    startAutoSlide();

    const arrowLeft = document.getElementById("slide-left");
    if (arrowLeft) {
      arrowLeft.addEventListener("click", () => {
        stopAutoSlide();
        const prevIndex = (currentSlideIndex - 1 + slides.length) % slides.length;
        updateSlide(prevIndex);
        startAutoSlide();
      });
    }

    const arrowRight = document.getElementById("slide-right");
    if (arrowRight) {
      arrowRight.addEventListener("click", () => {
        stopAutoSlide();
        const nextIndex = (currentSlideIndex + 1) % slides.length;
        updateSlide(nextIndex);
        startAutoSlide();
      });
    }

    const dotElements = document.querySelectorAll(".hero-dots .hero-dot");
    dotElements.forEach(dot => {
      dot.addEventListener("click", () => {
        stopAutoSlide();
        const targetIndex = parseInt(dot.getAttribute("data-index"), 10);
        updateSlide(targetIndex);
        startAutoSlide();
      });
    });
  }

  // Parallax Scroll & Scroll Spy auto-navigation indicator update
  const spySections = [
    { id: "home", linkSelector: "nav a[href='index.html']" },
    { id: "featured-properties", linkSelector: "nav a[href='#featured-properties']" },
    { id: "services", linkSelector: "nav a[href='#services']" },
    { id: "enquiry-desk", linkSelector: "nav a[href='#enquiry-desk']" }
  ];

  window.addEventListener("scroll", () => {
    // Parallax
    const depth = 0.35;
    const heroCardObj = document.querySelector(".hero-card");
    const scrollPos = window.pageYOffset || document.documentElement.scrollTop;
    if (heroCardObj) {
      heroCardObj.style.backgroundPositionY = `${scrollPos * depth}px`;
    }

    // Scroll Spy active navigation indicator
    let currentActiveId = "home";
    spySections.forEach(sec => {
      if (sec.id === "home") return;
      const el = document.getElementById(sec.id);
      if (el) {
        // If the section top is close to the top of viewport (offset offsetTop by header height)
        const offsetTop = el.offsetTop - 180;
        if (scrollPos >= offsetTop) {
          currentActiveId = sec.id;
        }
      }
    });

    spySections.forEach(sec => {
      const link = document.querySelector(sec.linkSelector);
      if (link) {
        if (sec.id === currentActiveId) {
          link.classList.add("active");
        } else {
          link.classList.remove("active");
        }
      }
    });
  });

  // Testimonials Carousel
  function initializeTestimonialCarousel() {
    const slidesList = document.querySelectorAll(".testimonial-slide");
    if (slidesList.length === 0) return;

    const testPrevBtn = document.getElementById("test-prev");
    const testNextBtn = document.getElementById("test-next");

    function showTestimonial(idx) {
      const activeSlides = document.querySelectorAll(".testimonial-slide");
      if (activeSlides.length === 0) return;
      activeSlides.forEach((slide, sIdx) => {
        slide.style.display = sIdx === idx ? "block" : "none";
      });
      
      const dotsList = document.querySelectorAll("#test-dots .hero-dot");
      dotsList.forEach((dot, dIdx) => {
        if (dIdx === idx) {
          dot.classList.add("active");
        } else {
          dot.classList.remove("active");
        }
      });
      testIndex = idx;
    }

    function nextTestimonial() {
      const activeSlides = document.querySelectorAll(".testimonial-slide");
      if (activeSlides.length === 0) return;
      const nextIdx = (testIndex + 1) % activeSlides.length;
      showTestimonial(nextIdx);
    }

    function prevTestimonial() {
      const activeSlides = document.querySelectorAll(".testimonial-slide");
      if (activeSlides.length === 0) return;
      const prevIdx = (testIndex - 1 + activeSlides.length) % activeSlides.length;
      showTestimonial(prevIdx);
    }

    if (testPrevBtn && testNextBtn) {
      testPrevBtn.onclick = () => {
        prevTestimonial();
        resetTestAutoRotation();
      };
      testNextBtn.onclick = () => {
        nextTestimonial();
        resetTestAutoRotation();
      };
    }

    function startTestAutoRotation() {
      stopTestAutoRotation();
      testAutoInterval = setInterval(nextTestimonial, 5000);
    }

    function stopTestAutoRotation() {
      if (testAutoInterval) clearInterval(testAutoInterval);
    }

    function resetTestAutoRotation() {
      stopTestAutoRotation();
      startTestAutoRotation();
    }

    // Touch events for mobile swiping
    let touchStartX = 0;
    let touchEndX = 0;
    const testimonialsWrapper = document.getElementById("dyn-testimonials");
    if (testimonialsWrapper) {
      testimonialsWrapper.ontouchstart = (e) => {
        touchStartX = e.changedTouches[0].screenX;
      };
      
      testimonialsWrapper.ontouchend = (e) => {
        touchEndX = e.changedTouches[0].screenX;
        if (touchStartX - touchEndX > 50) {
          nextTestimonial();
          resetTestAutoRotation();
        }
        if (touchEndX - touchStartX > 50) {
          prevTestimonial();
          resetTestAutoRotation();
        }
      };

      testimonialsWrapper.onmouseenter = stopTestAutoRotation;
      testimonialsWrapper.onmouseleave = startTestAutoRotation;
    }

    showTestimonial(0);
    startTestAutoRotation();
  }

  // Scroll Reveal Observer
  function initializeScrollReveals() {
    const scrollReveals = document.querySelectorAll(".reveal-on-scroll");
    const observerOptions = {
      root: null,
      threshold: 0.1,
      rootMargin: "0px 0px -50px 0px"
    };

    const revealObserver = new IntersectionObserver((entries, observer) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          entry.target.classList.add("revealed");
          
          const counters = entry.target.querySelectorAll(".counter-val");
          if (counters.length > 0) {
            animateCounters(counters);
          }
          observer.unobserve(entry.target);
        }
      });
    }, observerOptions);

    scrollReveals.forEach(el => {
      revealObserver.observe(el);
    });
  }

  // Counter count up animation
  function animateCounters(counters) {
    counters.forEach(counter => {
      if (counter.classList.contains("animating")) return;
      counter.classList.add("animating");

      const target = parseFloat(counter.getAttribute("data-target"));
      const prefix = counter.getAttribute("data-prefix") || "";
      const suffix = counter.getAttribute("data-suffix") || "";
      const isDecimal = target % 1 !== 0;
      
      const duration = 2000; 
      const startTime = performance.now();

      function updateCounter(currentTime) {
        const elapsed = currentTime - startTime;
        const progress = Math.min(elapsed / duration, 1);
        
        const easeProgress = progress * (2 - progress);
        const currentValue = easeProgress * target;

        if (isDecimal) {
          counter.textContent = prefix + currentValue.toFixed(1) + suffix;
        } else {
          counter.textContent = prefix + Math.floor(currentValue).toLocaleString() + suffix;
        }

        if (progress < 1) {
          requestAnimationFrame(updateCounter);
        } else {
          if (isDecimal) {
            counter.textContent = prefix + target.toFixed(1) + suffix;
          } else {
            counter.textContent = prefix + target.toLocaleString() + suffix;
          }
          counter.classList.remove("animating");
        }
      }

      requestAnimationFrame(updateCounter);
    });
  }

  // Close gateway modal when clicking backdrop
  const gateModal = document.getElementById("category-gateway-modal");
  if (gateModal) {
    gateModal.addEventListener("click", (e) => {
      if (e.target === gateModal) {
        gateModal.style.display = "none";
      }
    });
  }

  // Close service details modal when clicking backdrop
  const srvDetailsModal = document.getElementById("service-details-modal");
  if (srvDetailsModal) {
    srvDetailsModal.addEventListener("click", (e) => {
      if (e.target === srvDetailsModal) {
        srvDetailsModal.style.display = "none";
      }
    });
  }

  // Service Details Custom Database
  const serviceDetails = {
    "Property Listings": {
      icon: "📄",
      subtitle: "Comprehensive property catalog",
      details: [
        "<strong>Verified Listings:</strong> 100% of our properties are physically audited by local agents.",
        "<strong>Immersive Media:</strong> High-definition photos, layouts, and virtual tours to inspect from home.",
        "<strong>Smart Filters:</strong> Easily find options by configuration, budget, facing direction, and possession status."
      ]
    },
    "Home Loans": {
      icon: "🏦",
      subtitle: "Flexible financing partnerships",
      details: [
        "<strong>Direct Bank Tie-ups:</strong> Partnered with major public and private sector banks for premium rates.",
        "<strong>Low Interest Rates:</strong> Access exclusive packages starting at 8.25% with zero processing charges.",
        "<strong>Digital Single-Window:</strong> Submit documents online and track status right from your dashboard."
      ]
    },
    "Legal Assistance": {
      icon: "⚖️",
      subtitle: "Secure transaction compliance",
      details: [
        "<strong>Title Scrutiny:</strong> Legal verification of ownership documents, parent deeds, and EC checks.",
        "<strong>Agreement Drafting:</strong> Precision drafting of Sale Agreements, Lease Deeds, and registration drafts.",
        "<strong>Registration Support:</strong> Real-time liaison at the Sub-Registrar's office for booking execution."
      ]
    },
    "Property Valuation": {
      icon: "📈",
      subtitle: "Accurate comparative market reports",
      details: [
        "<strong>AI Appraisal:</strong> Data-driven property pricing engine based on recent registry transaction records.",
        "<strong>Locality Statistics:</strong> Deep dive reports into annual pricing trends and area development updates.",
        "<strong>Certified Reports:</strong> Instantly generate PDF valuation dossiers for bank approval or mortgage evaluation."
      ]
    }
  };

  // Global show service details function
  window.showServiceDetails = (linkEl) => {
    const card = linkEl.closest(".service-card");
    if (!card) return;
    
    const title = card.querySelector("h3")?.textContent.trim() || "";
    const iconText = card.querySelector(".service-icon-box")?.textContent.trim() || "";
    const descText = card.querySelector("p")?.textContent.trim() || "";
    
    const info = serviceDetails[title] || {
      icon: iconText || "⭐",
      subtitle: "Premium Professional Service",
      details: [
        `<strong>Support Desk:</strong> 24/7 priority customer support for ${title}.`,
        `<strong>Custom Consultation:</strong> Dedicated advisors to consult on your exact requirements.`,
        `<strong>Description:</strong> ${descText}`
      ]
    };
    
    document.getElementById("modal-service-icon-box").textContent = info.icon;
    document.getElementById("modal-service-title").textContent = title;
    document.getElementById("modal-service-subtitle").textContent = info.subtitle;
    
    const listContainer = document.getElementById("modal-service-details-list");
    listContainer.innerHTML = info.details.map(item => `
      <div style="display:flex; gap:12px; align-items:flex-start;">
        <span style="color:var(--accent); font-weight:bold; margin-top:2px;">✓</span>
        <div>${item}</div>
      </div>
    `).join("");
    
    document.getElementById("service-details-modal").style.display = "flex";
  };

  // Register the premium initialization callback globally
  window.initializePremiumFeatures = () => {
    initializeHeroSlider();
    initializeTestimonialCarousel();
    initializeScrollReveals();
  };

  // Run initial hookup in case some elements are already present
  window.initializePremiumFeatures();
}

export function runVanillaLogic() { initAppJs(); }

