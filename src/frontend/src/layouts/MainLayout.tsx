import { Outlet, Link, useLocation, useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import {
    LayoutDashboard,
    ShoppingCart,
    Users,
    Truck,
    Warehouse,
    BarChart3,
    Settings,
    LogOut,
    Menu,
    ChevronDown,
    Sun,
    Moon,
    Monitor,
    Wallet,
    Factory,
    UserCog,
    Building2,
    Folder,
} from 'lucide-react';
import { useAuthStore, useThemeStore, useSidebarStore } from '@/stores';
import { LanguageSwitcher } from '@/components/LanguageSwitcher';

interface NavItem {
    labelKey: string;
    icon: React.ReactNode;
    href?: string;
    children?: { labelKey: string; href: string }[];
}

const navItems: NavItem[] = [
    {
        labelKey: 'nav.dashboard',
        icon: <LayoutDashboard className="w-5 h-5" />,
        href: '/dashboard',
    },
    {
        labelKey: 'nav.systemCore',
        icon: <Building2 className="w-5 h-5" />,
        children: [
            { labelKey: 'nav.users', href: '/users' },
            { labelKey: 'nav.roles', href: '/roles' },
            { labelKey: 'nav.currencies', href: '/currencies' },
            { labelKey: 'nav.languages', href: '/languages' },
        ],
    },
    {
        labelKey: 'nav.crm',
        icon: <Users className="w-5 h-5" />,
        children: [
            { labelKey: 'nav.businessPartners', href: '/partners' },
            { labelKey: 'contacts.title', href: '/contacts' },
            { labelKey: 'leads.title', href: '/leads' },
            { labelKey: 'opportunities.title', href: '/opportunities' },
            { labelKey: 'tags.title', href: '/tags' },
            { labelKey: 'tickets.title', href: '/tickets' },
        ],
    },
    {
        labelKey: 'nav.inventory',
        icon: <Warehouse className="w-5 h-5" />,
        children: [
            { labelKey: 'nav.products', href: '/inventory/products' },
            { labelKey: 'nav.categories', href: '/inventory/categories' },
            { labelKey: 'nav.warehouses', href: '/inventory/warehouses' },
            { labelKey: 'nav.brands', href: '/inventory/brands' },
            { labelKey: 'inventory.stockLevels', href: '/inventory/levels' },
            { labelKey: 'inventory.stockMovements', href: '/inventory/movements' },
            { labelKey: 'nav.uom', href: '/inventory/uom' },
        ],
    },
    {
        labelKey: 'nav.sales',
        icon: <ShoppingCart className="w-5 h-5" />,
        children: [
            { labelKey: 'nav.quotes', href: '/sales/quotes' },
            { labelKey: 'nav.orders', href: '/sales/orders' },
            { labelKey: 'nav.invoices', href: '/sales/invoices' },
            { labelKey: 'nav.shipments', href: '/sales/shipments' },
        ],
    },
    {
        labelKey: 'nav.procurement',
        icon: <Truck className="w-5 h-5" />,
        children: [
            { labelKey: 'nav.requisitions', href: '/procurement/requisitions' },
            { labelKey: 'procurement.rfqs', href: '/procurement/rfqs' },
            { labelKey: 'procurement.purchaseQuotes', href: '/procurement/purchase-quotes' },
            { labelKey: 'nav.purchaseOrders', href: '/procurement/purchase-orders' },
            { labelKey: 'nav.goodsReceipts', href: '/procurement/goods-receipts' },
            { labelKey: 'procurement.qualityControl', href: '/procurement/qc' },
            { labelKey: 'procurement.purchaseReturns', href: '/procurement/returns' },
        ],
    },
    {
        labelKey: 'nav.finance',
        icon: <Wallet className="w-5 h-5" />,
        children: [
            { labelKey: 'nav.accounts', href: '/finance/accounts' },
            { labelKey: 'nav.journalEntries', href: '/finance/journal-entries' },
            { labelKey: 'nav.payments', href: '/finance/payments' },
            { labelKey: 'nav.exchangeRates', href: '/finance/rates' },
            { labelKey: 'nav.cheques', href: '/finance/cheques' },
        ],
    },
    {
        labelKey: 'nav.hr',
        icon: <UserCog className="w-5 h-5" />,
        children: [
            { labelKey: 'nav.employees', href: '/hr/employees' },
            { labelKey: 'nav.departments', href: '/hr/departments' },
            { labelKey: 'nav.leaveRequests', href: '/hr/leave' },
            { labelKey: 'nav.payroll', href: '/hr/payroll' },
            { labelKey: 'nav.workShifts', href: '/hr/work-shifts' },
            { labelKey: 'nav.advanceRequests', href: '/hr/advance-requests' },
            { labelKey: 'hr.publicHolidays', href: '/hr/public-holidays' },
            { labelKey: 'hr.commissionRules', href: '/hr/commission-rules' },
            { labelKey: 'hr.attendanceLogs', href: '/hr/attendance-logs' },
        ],
    },
    {
        labelKey: 'nav.fixedAssets',
        icon: <Building2 className="w-5 h-5" />,
        children: [
            { labelKey: 'nav.assets', href: '/fixed-assets/assets' },
            { labelKey: 'nav.assetCategories', href: '/fixed-assets/categories' },
        ],
    },
    {
        labelKey: 'nav.manufacturing',
        icon: <Factory className="w-5 h-5" />,
        children: [
            { labelKey: 'nav.bom', href: '/manufacturing/bom' },
            { labelKey: 'nav.productionOrders', href: '/manufacturing/orders' },
        ],
    },
    {
        labelKey: 'projects.title',
        icon: <Folder className="w-5 h-5" />,
        href: '/projects',
    },
    {
        labelKey: 'nav.reports',
        icon: <BarChart3 className="w-5 h-5" />,
        children: [
            { labelKey: 'nav.salesReports', href: '/reports/sales' },
            { labelKey: 'nav.inventoryReports', href: '/reports/inventory' },
            { labelKey: 'nav.financialReports', href: '/reports/finance' },
        ],
    },
    {
        labelKey: 'nav.settings',
        icon: <Settings className="w-5 h-5" />,
        children: [
            { labelKey: 'nav.general', href: '/settings' },
            { labelKey: 'nav.tenant', href: '/settings/tenant' },
        ],
    },
];

function cn(...classes: (string | boolean | undefined)[]) {
    return classes.filter(Boolean).join(' ');
}

export function MainLayout() {
    const { t } = useTranslation();
    const location = useLocation();
    const navigate = useNavigate();
    const { user, logout } = useAuthStore();
    const { theme, setTheme } = useThemeStore();
    const { isCollapsed, toggle } = useSidebarStore();
    const [expandedItems, setExpandedItems] = useState<string[]>([]);
    const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

    useEffect(() => {
        navItems.forEach((item) => {
            if (item.children?.some((child) => location.pathname === child.href)) {
                setExpandedItems((prev) =>
                    prev.includes(item.labelKey) ? prev : [...prev, item.labelKey]
                );
            }
        });
    }, [location.pathname]);

    const toggleExpand = (labelKey: string) => {
        setExpandedItems((prev) =>
            prev.includes(labelKey)
                ? prev.filter((item) => item !== labelKey)
                : [...prev, labelKey]
        );
    };

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    const themeIcons = {
        light: <Sun className="w-4 h-4" />,
        dark: <Moon className="w-4 h-4" />,
        system: <Monitor className="w-4 h-4" />,
    };

    const cycleTheme = () => {
        const themes: ('light' | 'dark' | 'system')[] = ['light', 'dark', 'system'];
        const currentIndex = themes.indexOf(theme);
        const nextTheme = themes[(currentIndex + 1) % themes.length];
        setTheme(nextTheme);
    };

    return (
        <div className="flex h-screen bg-[hsl(var(--background))]">
            {/* Sidebar */}
            <aside
                className={cn(
                    'fixed inset-y-0 left-0 z-50 flex flex-col bg-[hsl(var(--card))] border-r transition-all duration-300',
                    isCollapsed ? 'w-16' : 'w-64',
                    mobileMenuOpen ? 'translate-x-0' : '-translate-x-full md:translate-x-0'
                )}
            >
                {/* Logo */}
                <div className="flex items-center justify-between h-16 px-4 border-b">
                    {!isCollapsed && (
                        <span className="text-lg font-semibold bg-gradient-to-r from-indigo-500 to-purple-600 bg-clip-text text-transparent">
                            ModulerERP
                        </span>
                    )}
                    <button
                        onClick={toggle}
                        className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] hidden md:flex"
                    >
                        <Menu className="w-5 h-5" />
                    </button>
                </div>

                {/* Navigation */}
                <nav className="flex-1 overflow-y-auto p-2 space-y-1">
                    {navItems.map((item) => (
                        <div key={item.labelKey}>
                            {item.href ? (
                                <Link
                                    to={item.href}
                                    className={cn(
                                        'flex items-center gap-3 px-3 py-2 rounded-lg transition-colors',
                                        location.pathname === item.href
                                            ? 'bg-[hsl(var(--primary))] text-[hsl(var(--primary-foreground))]'
                                            : 'hover:bg-[hsl(var(--accent))]'
                                    )}
                                >
                                    {item.icon}
                                    {!isCollapsed && <span>{t(item.labelKey)}</span>}
                                </Link>
                            ) : (
                                <>
                                    <button
                                        onClick={() => toggleExpand(item.labelKey)}
                                        className={cn(
                                            'flex items-center justify-between w-full px-3 py-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors',
                                            item.children?.some((c) => location.pathname === c.href) &&
                                            'bg-[hsl(var(--accent))]'
                                        )}
                                    >
                                        <div className="flex items-center gap-3">
                                            {item.icon}
                                            {!isCollapsed && <span>{t(item.labelKey)}</span>}
                                        </div>
                                        {!isCollapsed && (
                                            <ChevronDown
                                                className={cn(
                                                    'w-4 h-4 transition-transform',
                                                    expandedItems.includes(item.labelKey) && 'rotate-180'
                                                )}
                                            />
                                        )}
                                    </button>
                                    {!isCollapsed && expandedItems.includes(item.labelKey) && (
                                        <div className="ml-8 mt-1 space-y-1">
                                            {item.children?.map((child) => (
                                                <Link
                                                    key={child.href}
                                                    to={child.href}
                                                    className={cn(
                                                        'block px-3 py-2 rounded-lg text-sm transition-colors',
                                                        location.pathname === child.href
                                                            ? 'bg-[hsl(var(--primary))] text-[hsl(var(--primary-foreground))]'
                                                            : 'hover:bg-[hsl(var(--accent))]'
                                                    )}
                                                >
                                                    {t(child.labelKey)}
                                                </Link>
                                            ))}
                                        </div>
                                    )}
                                </>
                            )}
                        </div>
                    ))}
                </nav>

                {/* Logout Button */}
                <div className="p-2 border-t">
                    <button
                        onClick={handleLogout}
                        className="flex items-center gap-3 w-full px-3 py-2 rounded-lg hover:bg-[hsl(var(--destructive))] hover:text-[hsl(var(--destructive-foreground))] transition-colors"
                    >
                        <LogOut className="w-5 h-5" />
                        {!isCollapsed && <span>{t('auth.signOut')}</span>}
                    </button>
                </div>
            </aside>

            {/* Mobile overlay */}
            {mobileMenuOpen && (
                <div
                    className="fixed inset-0 z-40 bg-black/50 md:hidden"
                    onClick={() => setMobileMenuOpen(false)}
                />
            )}

            {/* Main content */}
            <div
                className={cn(
                    'flex-1 flex flex-col transition-all duration-300',
                    isCollapsed ? 'md:ml-16' : 'md:ml-64'
                )}
            >
                {/* Header */}
                <header className="h-16 flex items-center justify-between px-4 border-b bg-[hsl(var(--card))]">
                    <button
                        onClick={() => setMobileMenuOpen(true)}
                        className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] md:hidden"
                    >
                        <Menu className="w-5 h-5" />
                    </button>

                    <div className="flex-1 md:flex-none" />

                    <div className="flex items-center gap-2">
                        <LanguageSwitcher />
                        <button
                            onClick={cycleTheme}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]"
                            title={`Theme: ${theme}`}
                        >
                            {themeIcons[theme]}
                        </button>

                        <div className="hidden md:flex items-center gap-2 ml-2">
                            <div className="w-8 h-8 rounded-full bg-gradient-to-r from-indigo-500 to-purple-600 text-white flex items-center justify-center text-sm font-medium">
                                {user?.firstName?.[0]}
                                {user?.lastName?.[0]}
                            </div>
                            <div className="text-sm">
                                <p className="font-medium">
                                    {user?.firstName} {user?.lastName}
                                </p>
                                <p className="text-[hsl(var(--muted-foreground))] text-xs">
                                    {user?.email}
                                </p>
                            </div>
                        </div>
                    </div>
                </header>

                {/* Page content */}
                <main className="flex-1 overflow-y-auto p-4 md:p-6">
                    <Outlet />
                </main>
            </div>
        </div>
    );
}
