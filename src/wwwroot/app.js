function toggleDrawer() {
    const isMobile = window.innerWidth < 960;

    if (isMobile) {
        let mobileNav = document.getElementById('mobile-nav-overlay');

        if (mobileNav) {
            closeMobileNav();
        } else {
            const navContent = document.querySelector('.mud-drawer .mud-navmenu');

            mobileNav = document.createElement('div');
            mobileNav.id = 'mobile-nav-overlay';
            mobileNav.style.cssText = `
                position: fixed;
                top: 64px;
                left: 0;
                width: 240px;
                height: calc(100% - 64px);
                background-color: #5D1E21;
                z-index: 9999;
                overflow-y: auto;
                transition: transform 0.3s ease;
            `;

            if (navContent) {
                mobileNav.appendChild(navContent.cloneNode(true));
            }

            // Close when any link inside is clicked
            mobileNav.addEventListener('click', function (e) {
                if (e.target.closest('a')) {
                    closeMobileNav();
                }
            });

            const backdrop = document.createElement('div');
            backdrop.id = 'drawer-backdrop';
            backdrop.style.cssText = 'position:fixed;top:64px;left:0;width:100%;height:100%;background:rgba(0,0,0,0.5);z-index:9998;';
            backdrop.onclick = closeMobileNav;

            document.body.appendChild(backdrop);
            document.body.appendChild(mobileNav);
        }
    } else {
        closeMobileNav(); // clean up if somehow open
        const drawer = document.querySelector('.mud-drawer');
        const layout = document.querySelector('.mud-layout');
        const isOpen = drawer?.classList.contains('mud-drawer--open');
        if (layout) layout.classList.toggle('mud-drawer-open-responsive-md-left');
        if (isOpen) {
            drawer.classList.remove('mud-drawer--open');
            drawer.style.transform = 'translateX(-300px)';
        } else {
            drawer.classList.add('mud-drawer--open');
            drawer.style.transform = 'translateX(0px)';
        }
    }
}

function closeMobileNav() {
    const mobileNav = document.getElementById('mobile-nav-overlay');
    const backdrop = document.getElementById('drawer-backdrop');
    if (mobileNav) mobileNav.remove();
    if (backdrop) backdrop.remove();
}

// Clean up mobile overlay if user resizes to desktop
window.addEventListener('resize', function () {
    if (window.innerWidth >= 960) {
        closeMobileNav();
    }
});