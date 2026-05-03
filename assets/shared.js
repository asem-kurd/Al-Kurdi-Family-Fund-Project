/**
 * صندوق التكافل العائلي - Shared JS Utilities
 * JWT Auth · API helpers · Toast notifications · RTL helpers
 */

/* ─── Config ─── */
const CONFIG = {
  API_BASE: '/api',          // ← change to your backend URL
  TOKEN_KEY: 'fund_token',
  USER_KEY:  'fund_user',
};

/* ─── Auth helpers ─── */
const Auth = {
  getToken() { return localStorage.getItem(CONFIG.TOKEN_KEY); },
  getUser()  { const u = localStorage.getItem(CONFIG.USER_KEY); return u ? JSON.parse(u) : null; },
  isLoggedIn(){ return !!this.getToken(); },
  hasRole(role) { const u = this.getUser(); return u && u.role === role; },
  isAdmin()  { return this.hasRole('Admin'); },
  isMember() { return this.hasRole('Member'); },

  save(token, user) {
    localStorage.setItem(CONFIG.TOKEN_KEY, token);
    localStorage.setItem(CONFIG.USER_KEY, JSON.stringify(user));
  },

  logout() {
    localStorage.removeItem(CONFIG.TOKEN_KEY);
    localStorage.removeItem(CONFIG.USER_KEY);
    window.location.href = 'login.html';
  },

  /** Call on every protected page — redirects to login if not auth'd */
  guard(requiredRole = null) {
    if (!this.isLoggedIn()) {
      window.location.href = 'login.html';
      return false;
    }
    if (requiredRole && !this.hasRole(requiredRole)) {
      Toast.error('ليس لديك صلاحية الوصول لهذه الصفحة');
      setTimeout(() => window.location.href = 'login.html', 1500);
      return false;
    }
    return true;
  },
};

/* ─── API helper ─── */
const API = {
  async request(method, endpoint, body = null) {
    const headers = {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
    };
    const token = Auth.getToken();
    if (token) headers['Authorization'] = `Bearer ${token}`;

    const opts = { method, headers };
    if (body) opts.body = JSON.stringify(body);

    try {
      const res = await fetch(`${CONFIG.API_BASE}${endpoint}`, opts);

      if (res.status === 401) { Auth.logout(); return null; }

      const data = await res.json();
      if (!res.ok) throw new Error(data.message || `HTTP ${res.status}`);
      return data;
    } catch (err) {
      console.error('API Error:', err);
      throw err;
    }
  },

  get(endpoint)           { return this.request('GET', endpoint); },
  post(endpoint, body)    { return this.request('POST', endpoint, body); },
  put(endpoint, body)     { return this.request('PUT', endpoint, body); },
  delete(endpoint)        { return this.request('DELETE', endpoint); },

  /* ── Specific endpoints ── */
  async login(email, password) {
    return this.post('/auth/login', { email, password });
  },
  async getDashboardStats()       { return this.get('/dashboard/stats'); },
  async getTransactions(params)   { return this.get(`/transactions?${new URLSearchParams(params)}`); },
  async getPayments(memberId)     { return this.get(`/payments/member/${memberId}`); },
  async getMembers()              { return this.get('/members'); },
  async getMeetings()             { return this.get('/meetings'); },
  async getWithdrawalRequests()   { return this.get('/withdrawals'); },
  async approveWithdrawal(id)     { return this.post(`/withdrawals/${id}/approve`); },
  async rejectWithdrawal(id, reason){ return this.post(`/withdrawals/${id}/reject`, { reason }); },
  async recordPayment(data)       { return this.post('/payments', data); },
  async createWithdrawal(data)    { return this.post('/withdrawals', data); },
};

/* ─── Toast notifications ─── */
const Toast = {
  _container: null,

  _init() {
    if (this._container) return;
    this._container = document.createElement('div');
    this._container.style.cssText = `
      position: fixed; top: 80px; left: 20px;
      z-index: 9999; display: flex; flex-direction: column; gap: 8px;
      pointer-events: none;
    `;
    document.body.appendChild(this._container);
  },

  show(msg, type = 'info', duration = 3500) {
    this._init();
    const colors = {
      success: { bg:'#EAF3DE', border:'#C0DD97', color:'#27500A', icon:'✓' },
      error:   { bg:'#FCEBEB', border:'#F7C1C1', color:'#791F1F', icon:'✗' },
      warning: { bg:'#FAEEDA', border:'#FAC775', color:'#633806', icon:'⚠' },
      info:    { bg:'#E6F1FB', border:'#85B7EB', color:'#0C447C', icon:'ℹ' },
    };
    const c = colors[type] || colors.info;

    const t = document.createElement('div');
    t.style.cssText = `
      background:${c.bg}; border:1px solid ${c.border}; color:${c.color};
      padding:11px 16px; border-radius:10px; font-size:13px; font-family:'Tajawal',sans-serif;
      display:flex; align-items:center; gap:8px;
      pointer-events:all; cursor:pointer; max-width:320px;
      direction:rtl; opacity:0; transform:translateX(-20px);
      transition:opacity .25s, transform .25s;
      box-shadow:0 4px 12px rgba(0,0,0,.1);
    `;
    t.innerHTML = `<span style="font-weight:700;font-size:15px;">${c.icon}</span> ${msg}`;
    this._container.appendChild(t);
    requestAnimationFrame(() => { t.style.opacity='1'; t.style.transform='translateX(0)'; });
    t.onclick = () => this._remove(t);
    setTimeout(() => this._remove(t), duration);
  },

  _remove(el) {
    el.style.opacity = '0'; el.style.transform = 'translateX(-20px)';
    setTimeout(() => el.remove(), 280);
  },

  success(msg) { this.show(msg, 'success'); },
  error(msg)   { this.show(msg, 'error'); },
  warning(msg) { this.show(msg, 'warning'); },
  info(msg)    { this.show(msg, 'info'); },
};

/* ─── Number formatting ─── */
const Fmt = {
  currency(n, decimals = 2) {
    return parseFloat(n).toFixed(decimals).replace(/\B(?=(\d{3})+(?!\d))/g, ',') + ' د.أ';
  },
  number(n) {
    return parseInt(n).toLocaleString('ar-JO');
  },
  percent(n) {
    return Math.round(n) + '%';
  },
  date(d) {
    if (!d) return '—';
    return new Date(d).toLocaleDateString('ar-JO', { year:'numeric', month:'long', day:'numeric' });
  },
  dateShort(d) {
    if (!d) return '—';
    return new Date(d).toLocaleDateString('ar-JO', { month:'short', day:'numeric', year:'numeric' });
  },
};

/* ─── UI helpers ─── */
const UI = {
  /** Inject current user info into [data-user-name] etc. elements */
  injectUserInfo() {
    const user = Auth.getUser();
    if (!user) return;
    document.querySelectorAll('[data-user-name]').forEach(el => el.textContent = user.fullName || user.name || 'المستخدم');
    document.querySelectorAll('[data-user-initials]').forEach(el => {
      const name = user.fullName || user.name || 'م';
      el.textContent = name.split(' ').map(w => w[0]).slice(0,2).join('');
    });
    document.querySelectorAll('[data-user-role]').forEach(el => {
      el.textContent = user.role === 'Admin' ? 'مدير' : 'عضو';
      el.className = user.role === 'Admin' ? 'role-badge role-admin' : 'role-badge role-member';
    });
  },

  /** Set active nav link based on current page */
  setActiveNav() {
    const page = window.location.pathname.split('/').pop();
    document.querySelectorAll('.nav-link[href]').forEach(a => {
      a.classList.toggle('active', a.getAttribute('href') === page);
    });
  },

  /** Show/hide loading spinner on a button */
  btnLoading(btn, loading, originalText) {
    if (loading) {
      btn.disabled = true;
      btn.dataset.originalText = btn.textContent;
      btn.textContent = '⏳ جاري التحميل...';
    } else {
      btn.disabled = false;
      btn.textContent = originalText || btn.dataset.originalText || btn.textContent;
    }
  },

  /** Generic confirm modal */
  confirm(message, onConfirm) {
    const overlay = document.createElement('div');
    overlay.style.cssText = `position:fixed;inset:0;background:rgba(0,0,0,.45);z-index:9000;display:flex;align-items:center;justify-content:center;`;
    overlay.innerHTML = `
      <div style="background:#fff;border-radius:14px;padding:2rem;width:340px;text-align:center;direction:rtl;font-family:'Tajawal',sans-serif;box-shadow:0 20px 60px rgba(0,0,0,.2);">
        <div style="font-size:36px;margin-bottom:12px;">⚠️</div>
        <p style="font-size:15px;color:#343a40;margin:0 0 1.5rem;">${message}</p>
        <div style="display:flex;gap:10px;justify-content:center;">
          <button id="cfm-yes" style="background:#1a3d5c;color:#fff;border:none;padding:9px 22px;border-radius:8px;font-size:14px;font-family:'Tajawal',sans-serif;cursor:pointer;font-weight:500;">تأكيد</button>
          <button id="cfm-no"  style="background:#f1f3f5;color:#495057;border:1px solid #dee2e6;padding:9px 22px;border-radius:8px;font-size:14px;font-family:'Tajawal',sans-serif;cursor:pointer;">إلغاء</button>
        </div>
      </div>`;
    document.body.appendChild(overlay);
    overlay.querySelector('#cfm-yes').onclick = () => { overlay.remove(); onConfirm(); };
    overlay.querySelector('#cfm-no').onclick  = () => overlay.remove();
    overlay.onclick = (e) => { if (e.target === overlay) overlay.remove(); };
  },
};

/* ─── Mobile Navigation ─── */
const MobileNav = {
  toggleNav() {
    const nav = document.querySelector('.nav-links');
    if (nav) nav.classList.toggle('open');
  },
  toggleSidebar() {
    const sidebar = document.querySelector('.sidebar');
    const overlay = document.querySelector('.sidebar-overlay');
    if (sidebar) sidebar.classList.toggle('open');
    if (overlay) overlay.classList.toggle('open');
  },
  closeSidebar() {
    const sidebar = document.querySelector('.sidebar');
    const overlay = document.querySelector('.sidebar-overlay');
    if (sidebar) sidebar.classList.remove('open');
    if (overlay) overlay.classList.remove('open');
  },
  closeNav() {
    const nav = document.querySelector('.nav-links');
    if (nav) nav.classList.remove('open');
  },
  init() {
    /* Close nav when clicking a link */
    document.querySelectorAll('.nav-link').forEach(link => {
      link.addEventListener('click', () => this.closeNav());
    });
    /* Close sidebar when clicking overlay */
    const overlay = document.querySelector('.sidebar-overlay');
    if (overlay) overlay.addEventListener('click', () => this.closeSidebar());
    /* Close on Escape key */
    document.addEventListener('keydown', e => {
      if (e.key === 'Escape') { this.closeSidebar(); this.closeNav(); }
    });
  },
};

/* ─── Init on every page load ─── */
document.addEventListener('DOMContentLoaded', () => {
  UI.injectUserInfo();
  UI.setActiveNav();
  MobileNav.init();
});
