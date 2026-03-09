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

    // ===== Calendar =====
    function setupCalendar() {
        const calendarContainer = document.getElementById('calendar');
        if (!calendarContainer) return;

        const now = new Date();
        const year = now.getFullYear();
        const month = now.getMonth();

        // Get first day of month and number of days
        const firstDay = new Date(year, month, 1);
        const lastDay = new Date(year, month + 1, 0);
        const daysInMonth = lastDay.getDate();
        const startingDayOfWeek = firstDay.getDay();

        const monthNames = ['January', 'February', 'March', 'April', 'May', 'June',
            'July', 'August', 'September', 'October', 'November', 'December'];

        let calendarHTML = `
        <div class="calendar">
            <div class="calendar-header">
                <button class="calendar-prev">← Previous</button>
                <h3>${monthNames[month]} ${year}</h3>
                <button class="calendar-next">Next →</button>
            </div>
            <div class="calendar-weekdays">
                <div>Sun</div>
                <div>Mon</div>
                <div>Tue</div>
                <div>Wed</div>
                <div>Thu</div>
                <div>Fri</div>
                <div>Sat</div>
            </div>
            <div class="calendar-days">
    `;

        // Empty cells before first day
        for (let i = 0; i < startingDayOfWeek; i++) {
            calendarHTML += '<div class="empty"></div>';
        }

        // Days of month
        for (let day = 1; day <= daysInMonth; day++) {
            const isToday = day === now.getDate() && month === now.getMonth();
            calendarHTML += `
            <div class="calendar-day ${isToday ? 'today' : ''}">
                ${day}
                <span class="event-dot"></span>
            </div>
        `;
        }

        calendarHTML += `
            </div>
        </div>
        <style>
            .calendar {
                max-width: 100%;
            }
            .calendar-header {
                display: flex;
                justify-content: space-between;
                align-items: center;
                margin-bottom: 2rem;
                gap: 1rem;
            }
            .calendar-header h3 {
                font-size: 1.25rem;
                font-weight: 600;
            }
            .calendar-header button {
                background: #3B82F6;
                color: white;
                border: none;
                padding: 0.5rem 1rem;
                border-radius: 0.5rem;
                cursor: pointer;
                font-size: 0.875rem;
            }
            .calendar-header button:hover {
                background: #60A5FA;
            }
            .calendar-weekdays {
                display: grid;
                grid-template-columns: repeat(7, 1fr);
                gap: 0.5rem;
                margin-bottom: 0.5rem;
                font-weight: 600;
                color: #6B7280;
                text-align: center;
                font-size: 0.875rem;
            }
            .calendar-days {
                display: grid;
                grid-template-columns: repeat(7, 1fr);
                gap: 0.5rem;
            }
            .calendar-day {
                aspect-ratio: 1;
                display: flex;
                align-items: center;
                justify-content: center;
                border: 1px solid #E5E7EB;
                border-radius: 0.5rem;
                cursor: pointer;
                font-size: 0.875rem;
                position: relative;
                transition: all 0.2s ease;
            }
            .calendar-day:hover {
                border-color: #3B82F6;
                background: #EFF6FF;
            }
            .calendar-day.today {
                background: #3B82F6;
                color: white;
                font-weight: 600;
            }
            .calendar-day.empty {
                cursor: default;
                border: none;
            }
            .event-dot {
                position: absolute;
                width: 4px;
                height: 4px;
                background: #EF4444;
                border-radius: 50%;
                bottom: 2px;
                display: none;
            }
            .calendar-day:nth-child(2) .event-dot,
            .calendar-day:nth-child(9) .event-dot,
            .calendar-day:nth-child(24) .event-dot {
                display: block;
            }
        </style>
    `;

        calendarContainer.innerHTML = calendarHTML;
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
