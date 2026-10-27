/* ==========================================================================
   Tagalong - Global Interactive Components
   ========================================================================== */

(function() {
  'use strict';

  // ==========================================================================
  // Lucide Icon Initialization
  // ==========================================================================
  function initLucide() {
    if (typeof lucide !== 'undefined') {
      lucide.createIcons({
        attrs: {
          'stroke-width': 2,
          'stroke-linecap': 'round',
          'stroke-linejoin': 'round'
        }
      });
    }
  }

  // Initialize on DOM ready
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initLucide);
  } else {
    initLucide();
  }

  // Re-initialize after dynamic content (HTMX, AJAX, etc.)
  document.addEventListener('htmx:afterSwap', initLucide);
  document.addEventListener('turbo:render', initLucide);

  // ==========================================================================
  // Dropdown Management
  // ==========================================================================
  const Dropdown = {
    openDropdowns: new Set(),

    init() {
      document.addEventListener('click', this.handleClick.bind(this));
      document.addEventListener('keydown', this.handleKeydown.bind(this));
    },

    handleClick(event) {
      const trigger = event.target.closest('[data-dropdown-trigger]');
      const dropdown = event.target.closest('[data-dropdown]');

      if (trigger) {
        event.preventDefault();
        event.stopPropagation();
        const targetId = trigger.getAttribute('data-dropdown-trigger');
        const target = document.getElementById(targetId);
        if (target) this.toggle(target);
      } else if (!dropdown) {
        this.closeAll();
      }
    },

    handleKeydown(event) {
      if (event.key === 'Escape') {
        this.closeAll();
      } else if (event.key === 'Tab') {
        this.handleTabNavigation(event);
      }
    },

    handleTabNavigation(event) {
      this.openDropdowns.forEach(dropdown => {
        const focusable = dropdown.querySelectorAll(
          'a[href], button, input, select, textarea, [tabindex]:not([tabindex="-1"])'
        );
        if (focusable.length === 0) return;

        const first = focusable[0];
        const last = focusable[focusable.length - 1];

        if (event.shiftKey && document.activeElement === first) {
          event.preventDefault();
          last.focus();
        } else if (!event.shiftKey && document.activeElement === last) {
          event.preventDefault();
          first.focus();
        }
      });
    },

    toggle(dropdown) {
      if (dropdown.classList.contains('show')) {
        this.close(dropdown);
      } else {
        this.closeAll();
        this.open(dropdown);
      }
    },

    open(dropdown) {
      dropdown.classList.add('show');
      dropdown.setAttribute('aria-hidden', 'false');
      const trigger = document.querySelector(`[data-dropdown-trigger="${dropdown.id}"]`);
      if (trigger) {
        trigger.setAttribute('aria-expanded', 'true');
      }
      this.openDropdowns.add(dropdown);

      // Position dropdown
      this.position(dropdown);

      // Focus first focusable element
      const focusable = dropdown.querySelector(
        'a[href], button, input, select, textarea, [tabindex]:not([tabindex="-1"])'
      );
      if (focusable) {
        setTimeout(() => focusable.focus(), 0);
      }
    },

    close(dropdown) {
      dropdown.classList.remove('show');
      dropdown.setAttribute('aria-hidden', 'true');
      const trigger = document.querySelector(`[data-dropdown-trigger="${dropdown.id}"]`);
      if (trigger) {
        trigger.setAttribute('aria-expanded', 'false');
        trigger.focus();
      }
      this.openDropdowns.delete(dropdown);
    },

    closeAll() {
      this.openDropdowns.forEach(dropdown => this.close(dropdown));
    },

    position(dropdown) {
      const trigger = document.querySelector(`[data-dropdown-trigger="${dropdown.id}"]`);
      if (!trigger) return;

      const rect = trigger.getBoundingClientRect();
      const dropdownRect = dropdown.getBoundingClientRect();
      const viewportWidth = window.innerWidth;
      const viewportHeight = window.innerHeight;

      // Reset positioning
      dropdown.style.top = '';
      dropdown.style.bottom = '';
      dropdown.style.left = '';
      dropdown.style.right = '';
      dropdown.style.transform = '';

      // Horizontal positioning
      if (rect.right + dropdownRect.width > viewportWidth) {
        dropdown.style.right = '0';
        dropdown.style.left = 'auto';
      } else {
        dropdown.style.left = '0';
        dropdown.style.right = 'auto';
      }

      // Vertical positioning (prefer below, flip if no space)
      const spaceBelow = viewportHeight - rect.bottom;
      if (spaceBelow < dropdownRect.height && rect.top > dropdownRect.height) {
        dropdown.style.bottom = '100%';
        dropdown.style.top = 'auto';
        dropdown.style.marginBottom = '0.5rem';
      } else {
        dropdown.style.top = '100%';
        dropdown.style.bottom = 'auto';
        dropdown.style.marginTop = '0.5rem';
      }
    }
  };

  Dropdown.init();

  // ==========================================================================
  // Password Visibility Toggle
  // ==========================================================================
  function initPasswordToggles() {
    document.querySelectorAll('[data-password-toggle]').forEach(toggle => {
      const targetId = toggle.getAttribute('data-password-toggle');
      const target = document.getElementById(targetId);
      if (!target) return;

      toggle.addEventListener('click', function(e) {
        e.preventDefault();
        const isHidden = target.type === 'password';
        target.type = isHidden ? 'text' : 'password';

        // Update icon
        const icon = toggle.querySelector('[data-lucide]');
        if (icon && typeof lucide !== 'undefined') {
          icon.setAttribute('data-lucide', isHidden ? 'eye-off' : 'eye');
          lucide.createIcons({ nodes: [icon] });
        }

        // Update aria-label
        toggle.setAttribute('aria-label', isHidden ? 'Hide password' : 'Show password');
        toggle.setAttribute('aria-pressed', isHidden.toString());
      });
    });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initPasswordToggles);
  } else {
    initPasswordToggles();
  }

  // Re-initialize after dynamic content
  document.addEventListener('htmx:afterSwap', initPasswordToggles);

  // ==========================================================================
  // Form Validation Enhancements
  // ==========================================================================
  const FormValidation = {
    init() {
      // Add validation classes on blur
      document.addEventListener('blur', this.handleBlur.bind(this), true);
      document.addEventListener('input', this.handleInput.bind(this), true);

      // Prevent form submission on Enter in non-submit inputs
      document.addEventListener('keydown', this.handleKeydown.bind(this));
    },

    handleBlur(event) {
      const input = event.target;
      if (!input.matches('input, select, textarea')) return;
      if (input.type === 'submit' || input.type === 'button') return;

      this.validateField(input);
    },

    handleInput(event) {
      const input = event.target;
      if (!input.matches('input, select, textarea')) return;

      // Clear validation on input if field was invalid
      if (input.classList.contains('is-invalid')) {
        this.validateField(input);
      }

      // Update character count if present
      const counter = document.getElementById(`${input.id}-count`);
      if (counter && input.hasAttribute('maxlength')) {
        counter.textContent = input.value.length;
      }
    },

    handleKeydown(event) {
      if (event.key === 'Enter' && event.target.tagName !== 'TEXTAREA') {
        const form = event.target.closest('form');
        if (form) {
          const submitBtn = form.querySelector('[type="submit"]:not([formnovalidate])');
          if (submitBtn && !submitBtn.disabled) {
            submitBtn.click();
            event.preventDefault();
          }
        }
      }
    },

    validateField(input) {
      const isValid = input.checkValidity();
      input.classList.toggle('is-valid', isValid && input.value !== '');
      input.classList.toggle('is-invalid', !isValid && input.value !== '');

      // Update associated error message
      const errorId = input.getAttribute('aria-describedby');
      if (errorId) {
        const errorEl = document.getElementById(errorId);
        if (errorEl) {
          errorEl.textContent = input.validationMessage;
        }
      }
    }
  };

  FormValidation.init();

  // ==========================================================================
  // Toast Notifications
  // ==========================================================================
  const Toast = {
    container: null,

    init() {
      this.container = document.querySelector('.toast-container');
      if (!this.container) {
        this.container = document.createElement('div');
        this.container.className = 'toast-container';
        document.body.appendChild(this.container);
      }
    },

    show(message, type = 'info', duration = 4000) {
      this.init();

      const toast = document.createElement('div');
      toast.className = `toast toast-${type}`;
      toast.setAttribute('role', 'alert');
      toast.setAttribute('aria-live', 'polite');

      const icons = {
        success: 'check-circle',
        error: 'alert-circle',
        warning: 'alert-triangle',
        info: 'info'
      };

      toast.innerHTML = `
        <i data-lucide="${icons[type]}" aria-hidden="true"></i>
        <span>${this.escapeHtml(message)}</span>
      `;

      if (typeof lucide !== 'undefined') {
        lucide.createIcons({ nodes: toast.querySelectorAll('[data-lucide]') });
      }

      this.container.appendChild(toast);

      // Auto-remove
      setTimeout(() => {
        toast.style.animation = 'slideIn 0.3s ease reverse';
        setTimeout(() => toast.remove(), 300);
      }, duration);

      return toast;
    },

    success(message, duration) { return this.show(message, 'success', duration); },
    error(message, duration) { return this.show(message, 'error', duration); },
    warning(message, duration) { return this.show(message, 'warning', duration); },
    info(message, duration) { return this.show(message, 'info', duration); },

    escapeHtml(text) {
      const div = document.createElement('div');
      div.textContent = text;
      return div.innerHTML;
    }
  };

  // Expose globally
  window.Toast = Toast;

  // ==========================================================================
  // Modal Enhancements
  // ==========================================================================
  const Modal = {
    init() {
      document.addEventListener('shown.bs.modal', this.onShown.bind(this));
      document.addEventListener('hidden.bs.modal', this.onHidden.bind(this));
      document.addEventListener('keydown', this.handleKeydown.bind(this));
    },

    onShown(event) {
      const modal = event.target;
      // Focus first focusable element
      const focusable = modal.querySelector(
        'input:not([type="hidden"]), select, textarea, button, [href], [tabindex]:not([tabindex="-1"])'
      );
      if (focusable) focusable.focus();

      // Trap focus
      modal._focusableElements = modal.querySelectorAll(
        'input:not([type="hidden"]), select, textarea, button, [href], [tabindex]:not([tabindex="-1"])'
      );
    },

    onHidden(event) {
      const modal = event.target;
      // Return focus to trigger
      const trigger = document.querySelector(`[data-bs-target="#${modal.id}"], [href="#${modal.id}"]`);
      if (trigger) trigger.focus();
    },

    handleKeydown(event) {
      if (event.key !== 'Tab') return;

      const modal = document.querySelector('.modal.show');
      if (!modal || !modal._focusableElements) return;

      const focusable = Array.from(modal._focusableElements).filter(el => {
        return el.offsetWidth > 0 && el.offsetHeight > 0 && !el.disabled;
      });

      if (focusable.length === 0) return;

      const first = focusable[0];
      const last = focusable[focusable.length - 1];

      if (event.shiftKey && document.activeElement === first) {
        event.preventDefault();
        last.focus();
      } else if (!event.shiftKey && document.activeElement === last) {
        event.preventDefault();
        first.focus();
      }
    }
  };

  Modal.init();

  // ==========================================================================
  // File Input Enhancement
  // ==========================================================================
  function initFileInputs() {
    document.querySelectorAll('input[type="file"]').forEach(input => {
      if (input.dataset.enhanced) return;
      input.dataset.enhanced = 'true';

      const wrapper = input.closest('.input-wrapper') || input.parentElement;
      const label = wrapper.querySelector('label') || document.createElement('label');

      input.addEventListener('change', function() {
        const fileName = this.files[0]?.name || 'No file chosen';
        const display = wrapper.querySelector('.file-name') || label;
        display.textContent = fileName;
        display.classList.add('has-file');
      });

      // Drag and drop
      ['dragenter', 'dragover'].forEach(eventName => {
        wrapper.addEventListener(eventName, e => {
          e.preventDefault();
          e.stopPropagation();
          wrapper.classList.add('drag-over');
        });
      });

      ['dragleave', 'drop'].forEach(eventName => {
        wrapper.addEventListener(eventName, e => {
          e.preventDefault();
          e.stopPropagation();
          wrapper.classList.remove('drag-over');
        });
      });

      wrapper.addEventListener('drop', e => {
        const files = e.dataTransfer.files;
        if (files.length) {
          input.files = files;
          input.dispatchEvent(new Event('change'));
        }
      });
    });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initFileInputs);
  } else {
    initFileInputs();
  }

  document.addEventListener('htmx:afterSwap', initFileInputs);

  // ==========================================================================
  // Auto-resize Textarea
  // ==========================================================================
  function initAutoResizeTextareas() {
    document.querySelectorAll('textarea[data-auto-resize]').forEach(textarea => {
      const resize = () => {
        textarea.style.height = 'auto';
        textarea.style.height = textarea.scrollHeight + 'px';
      };

      textarea.addEventListener('input', resize);
      resize(); // Initial resize
    });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initAutoResizeTextareas);
  } else {
    initAutoResizeTextareas();
  }

  document.addEventListener('htmx:afterSwap', initAutoResizeTextareas);

  // ==========================================================================
  // Smooth Scroll Anchor Links
  // ==========================================================================
  function initSmoothScroll() {
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
      anchor.addEventListener('click', function(e) {
        const targetId = this.getAttribute('href');
        if (targetId === '#') return;

        const target = document.querySelector(targetId);
        if (target) {
          e.preventDefault();
          const navbar = document.querySelector('.navbar.fixed-top');
          const offset = navbar ? navbar.offsetHeight : 0;

          window.scrollTo({
            top: target.offsetTop - offset - 20,
            behavior: 'smooth'
          });

          // Update URL without scroll
          history.pushState(null, '', targetId);
          target.focus({ preventScroll: true });
        }
      });
    });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initSmoothScroll);
  } else {
    initSmoothScroll();
  }

  // ==========================================================================
  // Reduced Motion Support
  // ==========================================================================
  const ReducedMotion = {
    prefersReduced: window.matchMedia('(prefers-reduced-motion: reduce)'),

    init() {
      this.prefersReduced.addEventListener('change', this.handleChange.bind(this));
      this.applyPreference(this.prefersReduced.matches);
    },

    handleChange(event) {
      this.applyPreference(event.matches);
    },

    applyPreference(reduced) {
      document.documentElement.style.setProperty(
        '--transition-base',
        reduced ? '0.01ms' : '200ms'
      );
      document.documentElement.style.setProperty(
        '--transition-slow',
        reduced ? '0.01ms' : '300ms'
      );
    }
  };

  ReducedMotion.init();

  // ==========================================================================
  // Theme Toggle (if needed)
  // ==========================================================================
  const Theme = {
    init() {
      const toggle = document.getElementById('themeToggle');
      if (!toggle) return;

      const savedTheme = localStorage.getItem('theme') ||
        (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');

      this.apply(savedTheme);

      toggle.addEventListener('click', () => {
        const newTheme = document.documentElement.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
        this.apply(newTheme);
      });

      // Listen for system changes
      window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
        if (!localStorage.getItem('theme')) {
          this.apply(e.matches ? 'dark' : 'light');
        }
      });
    },

    apply(theme) {
      document.documentElement.setAttribute('data-theme', theme);
      localStorage.setItem('theme', theme);

      const toggle = document.getElementById('themeToggle');
      if (toggle) {
        const icon = toggle.querySelector('[data-lucide]');
        if (icon && typeof lucide !== 'undefined') {
          icon.setAttribute('data-lucide', theme === 'dark' ? 'sun' : 'moon');
          lucide.createIcons({ nodes: [icon] });
        }
      }
    }
  };

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => Theme.init());
  } else {
    Theme.init();
  }

  // ==========================================================================
  // Utility: Debounce
  // ==========================================================================
  function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
      const later = () => {
        clearTimeout(timeout);
        func(...args);
      };
      clearTimeout(timeout);
      timeout = setTimeout(later, wait);
    };
  }

  // ==========================================================================
  // Utility: Throttle
  // ==========================================================================
  function throttle(func, limit) {
    let inThrottle;
    return function(...args) {
      if (!inThrottle) {
        func(...args);
        inThrottle = true;
        setTimeout(() => inThrottle = false, limit);
      }
    };
  }

  // Export utilities
  window.TagalongUtils = {
    debounce,
    throttle,
    escapeHtml: Toast.escapeHtml
  };

  // ==========================================================================
  // HTMX Integration Helpers
  // ==========================================================================
  if (typeof htmx !== 'undefined') {
    // Re-initialize components after HTMX swaps
    document.body.addEventListener('htmx:afterSwap', function(event) {
      // Re-initialize Lucide icons in swapped content
      if (typeof lucide !== 'undefined') {
        lucide.createIcons({ nodes: event.detail.target.querySelectorAll('[data-lucide]') });
      }

      // Re-initialize password toggles
      initPasswordToggles();

      // Re-initialize file inputs
      initFileInputs();

      // Re-initialize auto-resize textareas
      initAutoResizeTextareas();

      // Trigger custom event for other scripts
      event.detail.target.dispatchEvent(new CustomEvent('tagalong:content-swapped'));
    });

    // Show loading state on HTMX requests
    document.body.addEventListener('htmx:beforeRequest', function(event) {
      const target = event.detail.elt;
      if (target.classList.contains('btn')) {
        target.classList.add('loading');
        target.disabled = true;
      }
    });

    document.body.addEventListener('htmx:afterRequest', function(event) {
      const target = event.detail.elt;
      if (target.classList.contains('btn')) {
        target.classList.remove('loading');
        target.disabled = false;
      }
    });
  }

  // ==========================================================================
  // Expose Components Globally
  // ==========================================================================
  window.Tagalong = {
    Dropdown,
    Toast,
    Modal,
    ReducedMotion,
    Theme,
    utils: window.TagalongUtils
  };

})();