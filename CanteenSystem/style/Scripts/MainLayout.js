    // ===== Navigation =====
    function setupNavigation() {
        const navItems = document.querySelectorAll('.nav-item');
        const sections = document.querySelectorAll('.section');

        navItems.forEach(item => {
            item.addEventListener('click', (e) => {
                const sectionName = item.dataset.section;

                // Remove active class from all nav items and sections
                navItems.forEach(nav => nav.classList.remove('active'));
                sections.forEach(section => section.classList.remove('active'));

                // Add active class to clicked nav item and corresponding section
                item.classList.add('active');
                const targetSection = document.getElementById(`${sectionName}-section`);
                if (targetSection) {
                    targetSection.classList.add('active');

                    // Initialize charts if this is dashboard
                    if (sectionName === 'dashboard') {
                        setTimeout(initCharts, 100);
                    }
                }

                // Close sidebar on mobile
                sidebar.classList.remove('open');
            });
        });
    }

       // ===== Sidebar Toggle =====
    function setupSidebarToggle() {
        const sidebar = document.getElementById('sidebar');
        const mobileSidebarToggle = document.getElementById('mobileSidebarToggle');
        const menuToggle = document.getElementById('menuToggle');

        function toggleSidebar() {
            sidebar.classList.toggle('open');
        }

        if (mobileSidebarToggle) {
            mobileSidebarToggle.addEventListener('click', toggleSidebar);
        }

        if (menuToggle) {
            menuToggle.addEventListener('click', toggleSidebar);
        }

        // Close sidebar when clicking outside
        document.addEventListener('click', (e) => {
            if (!sidebar.contains(e.target) && !mobileSidebarToggle?.contains(e.target) && !menuToggle?.contains(e.target)) {
                sidebar.classList.remove('open');
            }
        });
    }

    // ===== Search Functionality =====
    function setupSearch() {
        const searchInput = document.querySelector('.search-input');
        if (!searchInput) return;

        searchInput.addEventListener('input', (e) => {
            const query = e.target.value.toLowerCase();
            const rows = document.querySelectorAll('.data-table tbody tr');

            rows.forEach(row => {
                const text = row.textContent.toLowerCase();
                row.style.display = text.includes(query) ? '' : 'none';
            });
        });
    }

    // ===== Mobile Responsive =====
    function setupResponsive() {
        const sidebar = document.getElementById('sidebar');

        window.addEventListener('resize', () => {
            if (window.innerWidth > 768) {
                sidebar.classList.remove('open');
            }
        });
    }

    // ===== Initialize Everything =====
    document.addEventListener('DOMContentLoaded', () => {
        setupNavigation();
        setupSidebarToggle();
        setupSearch();
        setupCalendar();
        setupResponsive();
    });

    // Optional: Add keyboard shortcuts
    document.addEventListener('keydown', (e) => {
        // Ctrl/Cmd + K for search
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            document.querySelector('.search-input')?.focus();
        }
    });
