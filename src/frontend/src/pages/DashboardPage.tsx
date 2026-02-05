import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
    DollarSign, Package, Users, Shield,
    Globe, Building2, ShoppingCart, Truck, Wallet,
    UserCog, Factory, BarChart3, ArrowUpRight
} from 'lucide-react';
import { api } from '@/services/api';

interface SystemSummary {
    currencies: number;
    languages: number;
    users: number;
    roles: number;
}

interface HealthStatus {
    status: string;
    database: string;
    timestamp: string;
}

export function DashboardPage() {
    const { t } = useTranslation();
    const [summary, setSummary] = useState<SystemSummary | null>(null);
    const [health, setHealth] = useState<HealthStatus | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        async function loadData() {
            const [summaryRes, healthRes] = await Promise.all([
                api.summary(),
                api.health()
            ]);

            if (summaryRes.success && summaryRes.data) {
                setSummary(summaryRes.data);
            }
            if (healthRes.success && healthRes.data) {
                setHealth(healthRes.data);
            }
            setLoading(false);
        }
        loadData();
    }, []);

    if (loading) {
        return (
            <div className="flex items-center justify-center h-64">
                <div className="animate-spin w-8 h-8 border-4 border-indigo-500 border-t-transparent rounded-full" />
            </div>
        );
    }

    const stats = [
        { labelKey: 'nav.currencies', value: summary?.currencies ?? 0, icon: DollarSign, color: 'bg-green-100 dark:bg-green-900/20 text-green-600' },
        { labelKey: 'nav.languages', value: summary?.languages ?? 0, icon: Globe, color: 'bg-blue-100 dark:bg-blue-900/20 text-blue-600' },
        { labelKey: 'nav.users', value: summary?.users ?? 0, icon: Users, color: 'bg-purple-100 dark:bg-purple-900/20 text-purple-600' },
        { labelKey: 'nav.roles', value: summary?.roles ?? 0, icon: Shield, color: 'bg-orange-100 dark:bg-orange-900/20 text-orange-600' },
    ];

    const modules = [
        { nameKey: 'modules.systemCore', icon: Building2, entities: 22 },
        { nameKey: 'modules.crm', icon: Users, entities: 9 },
        { nameKey: 'modules.inventory', icon: Package, entities: 11 },
        { nameKey: 'modules.sales', icon: ShoppingCart, entities: 12 },
        { nameKey: 'modules.procurement', icon: Truck, entities: 8 },
        { nameKey: 'modules.finance', icon: Wallet, entities: 9 },
        { nameKey: 'modules.hr', icon: UserCog, entities: 8 },
        { nameKey: 'modules.fixedAssets', icon: Building2, entities: 3 },
        { nameKey: 'modules.manufacturing', icon: Factory, entities: 4 },
    ];

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">{t('dashboard.title')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">
                        {t('dashboard.subtitle')}
                    </p>
                </div>
                <div className="flex items-center gap-2">
                    <span className={`w-2 h-2 rounded-full ${health?.status === 'healthy' ? 'bg-green-500' : 'bg-red-500'}`} />
                    <span className="text-sm text-[hsl(var(--muted-foreground))]">
                        {health?.status === 'healthy' ? t('dashboard.systemOnline') : t('dashboard.systemOffline')}
                    </span>
                </div>
            </div>

            {/* Stats Cards */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                {stats.map((stat) => (
                    <div key={stat.labelKey} className="bg-[hsl(var(--card))] rounded-xl border p-5">
                        <div className="flex items-start justify-between">
                            <div>
                                <p className="text-sm text-[hsl(var(--muted-foreground))]">{t(stat.labelKey)}</p>
                                <p className="text-2xl font-bold mt-1">{stat.value}</p>
                                <div className="flex items-center gap-1 mt-2 text-sm text-green-600">
                                    <ArrowUpRight className="w-4 h-4" />
                                    <span>{t('common.active')}</span>
                                </div>
                            </div>
                            <div className={`p-3 rounded-xl ${stat.color}`}>
                                <stat.icon className="w-5 h-5" />
                            </div>
                        </div>
                    </div>
                ))}
            </div>

            {/* Modules Grid */}
            <div>
                <h2 className="text-lg font-semibold mb-4">{t('dashboard.activeModules')}</h2>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                    {modules.map((module) => (
                        <div key={module.nameKey} className="bg-[hsl(var(--card))] rounded-xl border p-5 hover:shadow-lg transition-shadow">
                            <div className="flex items-start justify-between mb-3">
                                <div className="p-2 rounded-lg bg-indigo-100 dark:bg-indigo-900/20 text-indigo-600">
                                    <module.icon className="w-5 h-5" />
                                </div>
                                <span className="px-2 py-1 text-xs font-medium bg-green-100 dark:bg-green-900/20 text-green-600 rounded-full">
                                    {t('common.active')}
                                </span>
                            </div>
                            <h3 className="font-semibold">{t(module.nameKey)}</h3>
                            <p className="text-sm text-[hsl(var(--muted-foreground))] mt-1">
                                {t('modules.entities', { count: module.entities })}
                            </p>
                        </div>
                    ))}
                </div>
            </div>

            {/* Info Cards */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <div className="bg-[hsl(var(--card))] rounded-xl border p-6">
                    <h3 className="font-semibold mb-4 flex items-center gap-2">
                        <Globe className="w-5 h-5 text-indigo-600" />
                        {t('dashboard.trncFeatures')}
                    </h3>
                    <ul className="space-y-2 text-sm">
                        <li className="flex items-center gap-2">
                            <span className="w-5 h-5 rounded-full bg-green-100 dark:bg-green-900/20 text-green-600 flex items-center justify-center text-xs">✓</span>
                            {t('dashboard.multiCurrency')}
                        </li>
                        <li className="flex items-center gap-2">
                            <span className="w-5 h-5 rounded-full bg-green-100 dark:bg-green-900/20 text-green-600 flex items-center justify-center text-xs">✓</span>
                            {t('dashboard.bilingual')}
                        </li>
                        <li className="flex items-center gap-2">
                            <span className="w-5 h-5 rounded-full bg-green-100 dark:bg-green-900/20 text-green-600 flex items-center justify-center text-xs">✓</span>
                            {t('dashboard.exchangeRateFreezing')}
                        </li>
                        <li className="flex items-center gap-2">
                            <span className="w-5 h-5 rounded-full bg-green-100 dark:bg-green-900/20 text-green-600 flex items-center justify-center text-xs">✓</span>
                            {t('dashboard.tcknSupport')}
                        </li>
                        <li className="flex items-center gap-2">
                            <span className="w-5 h-5 rounded-full bg-green-100 dark:bg-green-900/20 text-green-600 flex items-center justify-center text-xs">✓</span>
                            {t('dashboard.timezone')}
                        </li>
                    </ul>
                </div>

                <div className="bg-[hsl(var(--card))] rounded-xl border p-6">
                    <h3 className="font-semibold mb-4 flex items-center gap-2">
                        <BarChart3 className="w-5 h-5 text-indigo-600" />
                        {t('dashboard.systemInfo')}
                    </h3>
                    <ul className="space-y-2 text-sm">
                        <li className="flex items-center justify-between py-2 border-b border-[hsl(var(--border))]">
                            <span className="text-[hsl(var(--muted-foreground))]">{t('dashboard.database')}</span>
                            <span className={health?.database === 'connected' ? 'text-green-600' : 'text-red-600'}>
                                {health?.database === 'connected' ? t('dashboard.postgresConnected') : t('dashboard.disconnected')}
                            </span>
                        </li>
                        <li className="flex items-center justify-between py-2 border-b border-[hsl(var(--border))]">
                            <span className="text-[hsl(var(--muted-foreground))]">{t('dashboard.architecture')}</span>
                            <span>{t('dashboard.modularMonolith')}</span>
                        </li>
                        <li className="flex items-center justify-between py-2 border-b border-[hsl(var(--border))]">
                            <span className="text-[hsl(var(--muted-foreground))]">{t('dashboard.activeModules')}</span>
                            <span>9</span>
                        </li>
                        <li className="flex items-center justify-between py-2">
                            <span className="text-[hsl(var(--muted-foreground))]">{t('dashboard.totalEntities')}</span>
                            <span>86</span>
                        </li>
                    </ul>
                </div>
            </div>
        </div>
    );
}
