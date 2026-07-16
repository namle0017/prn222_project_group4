// FapWeb — Site-wide micro-interactions & enhancements

(function () {
  'use strict';

  // ──────────────────────────────────────────────
  // Active nav link highlight
  // ──────────────────────────────────────────────
  const currentPath = window.location.pathname.toLowerCase();
  document.querySelectorAll('.nav-link').forEach(function (link) {
    const href = (link.getAttribute('href') || '').toLowerCase();
    if (href && href !== '/' && currentPath.startsWith(href)) {
      link.classList.add('active');
    }
  });

  // ──────────────────────────────────────────────
  // Scroll reveal via IntersectionObserver
  // ──────────────────────────────────────────────
  if ('IntersectionObserver' in window) {
    const revealObserver = new IntersectionObserver(function (entries) {
      entries.forEach(function (entry) {
        if (entry.isIntersecting) {
          entry.target.classList.add('visible');
          revealObserver.unobserve(entry.target);
        }
      });
    }, { threshold: 0.1, rootMargin: '0px 0px -40px 0px' });

    document.querySelectorAll('.reveal').forEach(function (el) {
      revealObserver.observe(el);
    });
  } else {
    // Fallback: show all
    document.querySelectorAll('.reveal').forEach(function (el) {
      el.classList.add('visible');
    });
  }

  // ──────────────────────────────────────────────
  // Navbar scroll effect — increase opacity/blur on scroll
  // ──────────────────────────────────────────────
  var navbar = document.getElementById('main-navbar');
  if (navbar) {
    window.addEventListener('scroll', function () {
      if (window.scrollY > 30) {
        navbar.style.backdropFilter = 'blur(28px)';
        navbar.style.webkitBackdropFilter = 'blur(28px)';
      } else {
        navbar.style.backdropFilter = 'blur(20px)';
        navbar.style.webkitBackdropFilter = 'blur(20px)';
      }
    }, { passive: true });
  }

  // ──────────────────────────────────────────────
  // Button ripple effect
  // ──────────────────────────────────────────────
  document.querySelectorAll('.btn').forEach(function (btn) {
    btn.addEventListener('click', function (e) {
      var rect = btn.getBoundingClientRect();
      var ripple = document.createElement('span');
      var size = Math.max(rect.width, rect.height);
      var x = e.clientX - rect.left - size / 2;
      var y = e.clientY - rect.top - size / 2;

      ripple.style.cssText = [
        'position:absolute',
        'border-radius:50%',
        'background:rgba(255,255,255,0.22)',
        'pointer-events:none',
        'transform:scale(0)',
        'animation:ripple-anim 0.55s ease-out forwards',
        'width:' + size + 'px',
        'height:' + size + 'px',
        'left:' + x + 'px',
        'top:' + y + 'px'
      ].join(';');

      // Ensure btn has relative positioning for ripple
      var pos = window.getComputedStyle(btn).position;
      if (pos === 'static') btn.style.position = 'relative';
      btn.style.overflow = 'hidden';
      btn.appendChild(ripple);

      ripple.addEventListener('animationend', function () {
        ripple.remove();
      });
    });
  });

  // Inject ripple keyframes once
  if (!document.getElementById('ripple-style')) {
    var style = document.createElement('style');
    style.id = 'ripple-style';
    style.textContent = '@keyframes ripple-anim { to { transform: scale(3); opacity: 0; } }';
    document.head.appendChild(style);
  }

  // ──────────────────────────────────────────────
  // Auto-dismiss alerts after 5 seconds
  // ──────────────────────────────────────────────
  document.querySelectorAll('.alert:not(.alert-permanent)').forEach(function (alert) {
    setTimeout(function () {
      alert.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
      alert.style.opacity = '0';
      alert.style.transform = 'translateY(-8px)';
      setTimeout(function () { alert.remove(); }, 500);
    }, 5000);
  });

  // ──────────────────────────────────────────────
  // Table row hover cursor
  // ──────────────────────────────────────────────
  document.querySelectorAll('.table-hover tbody tr').forEach(function (row) {
    row.style.cursor = 'default';
  });

})();
