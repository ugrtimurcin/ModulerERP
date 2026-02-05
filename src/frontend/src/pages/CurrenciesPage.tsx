import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { api } from '@/services/api';
import { DataTable, Badge } from '@/components/ui';
import type { Column } from '@/components/ui';

interface Currency {
    id: string;
    code: string;
    name: string;
    symbol: string;
    precision: number;
    isActive: boolean;
}

export function CurrenciesPage() {
    const { t } = useTranslation();

    const [currencies, setCurrencies] = useState<Currency[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        async function load() {
            const result = await api.getCurrencies();
            if (result.success && result.data) {
                setCurrencies(result.data);
            }
            setIsLoading(false);
        }
        load();
    }, []);

    const columns: Column<Currency>[] = [
        {
            key: 'code',
            header: t('currencies.code'),
            render: (currency) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-green-100 dark:bg-green-900/30 text-green-600 flex items-center justify-center">
                        <span className="text-lg font-semibold">{currency.symbol}</span>
                    </div>
                    <div>
                        <p className="font-medium">{currency.code}</p>
                        <p className="text-sm text-[hsl(var(--muted-foreground))]">
                            {currency.name}
                        </p>
                    </div>
                </div>
            ),
        },
        {
            key: 'symbol',
            header: t('currencies.symbol'),
            render: (currency) => (
                <span className="text-lg font-mono">{currency.symbol}</span>
            ),
        },
        {
            key: 'precision',
            header: t('currencies.precision'),
            render: (currency) => (
                <span>{currency.precision} {t('currencies.precision').toLowerCase()}</span>
            ),
        },
        {
            key: 'isActive',
            header: t('common.status'),
            render: (currency) => (
                <Badge variant={currency.isActive ? 'success' : 'error'}>
                    {currency.isActive ? t('common.active') : t('common.inactive')}
                </Badge>
            ),
        },
    ];

    return (
        <div className="space-y-6">
            {/* Header */}
            <div>
                <h1 className="text-2xl font-bold">{t('currencies.title')}</h1>
                <p className="text-[hsl(var(--muted-foreground))]">
                    {t('currencies.subtitle')}
                </p>
            </div>

            {/* Table */}
            <DataTable
                data={currencies}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
            />
        </div>
    );
}
