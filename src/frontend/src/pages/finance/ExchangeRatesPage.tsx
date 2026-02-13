import React, { useEffect, useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Trash2, RefreshCw } from 'lucide-react';
import { api } from '../../services/api';
import { DataTable, Button, useToast, Modal, Input, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';

interface ExchangeRateDto {
    id: string;
    fromCurrencyId: string;
    toCurrencyId: string;
    rateDate: string;
    rate: number;
    buyingRate: number;
    sellingRate: number;
    source: string;
}

interface CurrencyDto {
    id: string;
    code: string;
    name: string;
}

const ExchangeRatesPage: React.FC = () => {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();
    const [rates, setRates] = useState<ExchangeRateDto[]>([]);
    const [currencies, setCurrencies] = useState<CurrencyDto[]>([]);
    const [loading, setLoading] = useState(true);

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [formData, setFormData] = useState({
        fromCurrencyId: '',
        toCurrencyId: '',
        rateDate: new Date().toISOString().split('T')[0],
        rate: 0,
        buyingRate: 0,
        sellingRate: 0
    });

    const loadData = useCallback(async () => {
        setLoading(true);
        try {
            const [ratesRes, currRes] = await Promise.all([
                api.finance.exchangeRates.getAll(),
                api.getCurrencies() // Use SystemCore API
            ]);

            if (ratesRes.success) setRates(ratesRes.data || []);
            if (currRes.success) setCurrencies(currRes.data || []);
        } catch (error) {
            console.error(error);
            toast.error(t('common.error'), t('common.noData'));
        } finally {
            setLoading(false);
        }
    }, [toast, t]);

    useEffect(() => {
        loadData();
    }, [loadData]);

    const handleCreate = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            const res = await api.finance.exchangeRates.create(formData);
            if (res.success) {
                toast.success(t('common.success'), t('finance.exchangeRates.rateCreated'));
                setIsModalOpen(false);
                loadData();
            } else {
                toast.error(t('common.error'), res.error || t('finance.exchangeRates.createFailed'));
            }
        } catch (error) {
            toast.error(t('common.error'), t('finance.exchangeRates.createFailed'));
        }
    };

    const handleDelete = async (id: string) => {
        const confirmed = await dialog.danger({
            title: t('finance.exchangeRates.deleteRate'),
            message: t('finance.exchangeRates.confirmDelete'),
            confirmText: t('common.delete'),
            cancelText: t('common.cancel')
        });

        if (!confirmed) return;

        try {
            const res = await api.finance.exchangeRates.delete(id);
            if (res.success) {
                toast.success(t('common.deletedSuccess'), t('finance.exchangeRates.rateDeleted'));
                loadData();
            }
        } catch (error) {
            toast.error(t('common.error'), t('finance.exchangeRates.deleteFailed'));
        }
    };

    const handleSync = async () => {
        setLoading(true);
        const date = new Date().toISOString().split('T')[0];
        let syncedCount = 0;
        let errors = 0;

        // Try to identify base currency (TRY/TL)
        // Usually target ID is the base currency.
        // We'll look for TRY or TL.
        const baseCurrency = currencies.find(c => c.code === 'TRY' || c.code === 'TL');
        if (!baseCurrency) {
            toast.error(t('common.error'), "Base currency (TRY) not found");
            setLoading(false);
            return;
        }

        const targetCodes = ['USD', 'EUR', 'GBP'];

        try {
            for (const code of targetCodes) {
                const currency = currencies.find(c => c.code === code);
                if (!currency) continue;

                try {
                    // 1. Fetch External Rate (Returns ExternalRateDto)
                    const fetchRes = await api.finance.exchangeRates.fetchExternal(date, code);

                    if (fetchRes.success && fetchRes.data) {
                        const rateData = fetchRes.data; // { rate, buyingRate, sellingRate, currencyCode }

                        // 2. Create Rate in DB
                        const createRes = await api.finance.exchangeRates.create({
                            fromCurrencyId: currency.id,
                            toCurrencyId: baseCurrency.id,
                            rateDate: date,
                            rate: rateData.rate,
                            buyingRate: rateData.buyingRate,
                            sellingRate: rateData.sellingRate,
                            source: 'KKTC MB'
                        });

                        if (createRes.success) {
                            syncedCount++;
                        } else {
                            console.warn(`Failed to create rate for ${code}:`, createRes.error);
                        }
                    } else {
                        console.warn(`Failed to fetch rate for ${code}:`, fetchRes.error);
                        errors++;
                    }
                } catch (err) {
                    console.error(`Error syncing ${code}`, err);
                    errors++;
                }
            }

            if (syncedCount > 0) {
                toast.success(t('common.success'), t('finance.exchangeRates.synced', { count: syncedCount }));
                loadData();
            } else if (errors > 0) {
                toast.warning(t('common.warning'), t('finance.exchangeRates.syncFailed'));
            } else {
                toast.info(t('common.info'), t('common.noData'));
            }

        } catch (error) {
            toast.error(t('common.error'), t('finance.exchangeRates.syncFailed'));
        } finally {
            setLoading(false);
        }
    };

    const getCurrencyCode = (id: string) => currencies.find(c => c.id === id)?.code || id;

    const columns: Column<ExchangeRateDto>[] = [
        {
            key: 'rateDate',
            header: t('finance.exchangeRates.date'),
            render: (row) => new Date(row.rateDate).toLocaleDateString()
        },
        {
            key: 'fromCurrencyId',
            header: t('finance.exchangeRates.pair'),
            render: (row) => (
                <span className="font-medium">
                    {getCurrencyCode(row.fromCurrencyId)} / {getCurrencyCode(row.toCurrencyId)}
                </span>
            )
        },
        {
            key: 'rate',
            header: t('finance.exchangeRates.rate'),
            align: 'right',
            render: (row) => row.rate.toFixed(4)
        },
        {
            key: 'buyingRate',
            header: t('finance.exchangeRates.buyingRate'),
            align: 'right',
            render: (row) => row.buyingRate?.toFixed(4) || '-'
        },
        {
            key: 'sellingRate',
            header: t('finance.exchangeRates.sellingRate'),
            align: 'right',
            render: (row) => row.sellingRate?.toFixed(4) || '-'
        },
        { key: 'source', header: t('finance.exchangeRates.source') },
        {
            key: 'id',
            header: t('common.actions'),
            align: 'right',
            render: (row) => (
                <button
                    onClick={() => handleDelete(row.id)}
                    className="p-2 text-red-500 hover:bg-red-50 rounded"
                >
                    <Trash2 className="w-4 h-4" />
                </button>
            )
        }
    ];

    const [fetchingRate, setFetchingRate] = useState(false);

    const handleFetchRate = async () => {
        if (!formData.fromCurrencyId || !formData.rateDate) {
            toast.error(t('common.error'), t('finance.exchangeRates.selectDateAndCurrency'));
            return;
        }

        const code = currencies.find(c => c.id === formData.fromCurrencyId)?.code;
        if (!code) return;

        setFetchingRate(true);
        try {
            const res = await api.finance.exchangeRates.fetchExternal(formData.rateDate, code);
            if (res.success && res.data) {
                const r = res.data;
                setFormData(prev => ({
                    ...prev,
                    rate: r.rate,
                    buyingRate: r.buyingRate,
                    sellingRate: r.sellingRate
                }));
                toast.success(t('common.updatedSuccess'), t('finance.exchangeRates.rateFetched', { code, rate: r.rate }));
            } else {
                toast.error(t('common.error'), res.error || t('finance.exchangeRates.rateNotFound'));
            }
        } catch (error) {
            toast.error(t('common.error'), t('finance.exchangeRates.fetchFailed'));
        } finally {
            setFetchingRate(false);
        }
    };

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-2xl font-bold">{t('nav.exchangeRates')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('finance.exchangeRates.subtitle')}</p>
                </div>
                <div className="flex gap-2">
                    <Button variant="secondary" onClick={handleSync}>
                        <RefreshCw className="w-4 h-4 mr-2" />
                        {t('finance.exchangeRates.syncKktc')}
                    </Button>
                    <Button onClick={() => {
                        setFormData({
                            fromCurrencyId: '',
                            toCurrencyId: '',
                            rateDate: new Date().toISOString().split('T')[0],
                            rate: 0,
                            buyingRate: 0,
                            sellingRate: 0
                        });
                        setIsModalOpen(true);
                    }}>
                        <Plus className="w-4 h-4 mr-2" />
                        {t('finance.exchangeRates.addRate')}
                    </Button>
                </div>
            </div>

            <DataTable
                data={rates}
                columns={columns}
                keyField="id"
                isLoading={loading}
            />

            <Modal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title={t('finance.exchangeRates.addRate')}>
                <form onSubmit={handleCreate} className="space-y-4">
                    <Input
                        label={t('finance.exchangeRates.date')}
                        type="date"
                        value={formData.rateDate}
                        onChange={e => setFormData({ ...formData, rateDate: e.target.value })}
                        required
                    />
                    <div>
                        <label className="block text-sm font-medium mb-1">{t('finance.exchangeRates.fromCurrency')}</label>
                        <select
                            className="w-full p-2 border rounded-md dark:bg-gray-800"
                            value={formData.fromCurrencyId}
                            onChange={e => {
                                const newFromId = e.target.value;
                                let newToId = formData.toCurrencyId;
                                const code = currencies.find(c => c.id === newFromId)?.code;
                                if (['USD', 'EUR', 'GBP'].includes(code || '')) {
                                    const tryId = currencies.find(c => c.code === 'TRY' || c.code === 'TL')?.id;
                                    if (tryId) newToId = tryId;
                                }
                                setFormData({
                                    ...formData,
                                    fromCurrencyId: newFromId,
                                    toCurrencyId: newToId,
                                    rate: 0,
                                    buyingRate: 0,
                                    sellingRate: 0
                                });
                            }}
                            required
                        >
                            <option value="">{t('common.select')}</option>
                            {currencies.map(c => (
                                <option key={c.id} value={c.id}>{c.code} - {c.name}</option>
                            ))}
                        </select>
                    </div>
                    <div>
                        <label className="block text-sm font-medium mb-1">{t('finance.exchangeRates.toCurrency')}</label>
                        <select
                            className="w-full p-2 border rounded-md dark:bg-gray-800"
                            value={formData.toCurrencyId}
                            onChange={e => setFormData({
                                ...formData,
                                toCurrencyId: e.target.value,
                                rate: 0,
                                buyingRate: 0,
                                sellingRate: 0
                            })}
                            required
                        >
                            <option value="">{t('common.select')}</option>
                            {currencies.map(c => (
                                <option key={c.id} value={c.id}>{c.code} - {c.name}</option>
                            ))}
                        </select>
                    </div>
                    <div className="flex gap-2 items-end">
                        <div className="flex-1">
                            <Input
                                label={t('finance.exchangeRates.rate')}
                                type="number"
                                step="0.0001"
                                value={formData.rate}
                                onChange={e => setFormData({ ...formData, rate: parseFloat(e.target.value) })}
                                required
                            />
                        </div>
                        <div className="flex-1">
                            <Input
                                label={t('finance.exchangeRates.buyingRate')}
                                type="number"
                                step="0.0001"
                                value={formData.buyingRate}
                                onChange={e => setFormData({ ...formData, buyingRate: parseFloat(e.target.value) })}
                                required
                            />
                        </div>
                        <div className="flex-1">
                            <Input
                                label={t('finance.exchangeRates.sellingRate')}
                                type="number"
                                step="0.0001"
                                value={formData.sellingRate}
                                onChange={e => setFormData({ ...formData, sellingRate: parseFloat(e.target.value) })}
                                required
                            />
                        </div>
                        <Button
                            type="button"
                            variant="secondary"
                            onClick={handleFetchRate}
                            disabled={fetchingRate}
                            title={t('finance.exchangeRates.fetchKktc')}
                            className="mb-1"
                        >
                            <RefreshCw className={`w-4 h-4 ${fetchingRate ? 'animate-spin' : ''}`} />
                        </Button>
                    </div>
                    <div className="flex justify-end gap-2 pt-4">
                        <Button variant="secondary" type="button" onClick={() => setIsModalOpen(false)}>{t('common.cancel')}</Button>
                        <Button type="submit">{t('common.save')}</Button>
                    </div>
                </form>
            </Modal>
        </div>
    );
};

export default ExchangeRatesPage;
