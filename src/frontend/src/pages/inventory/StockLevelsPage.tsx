import { useState, useEffect } from 'react';
import { api } from '@/services/api';
import { useToast, Button, Input, Select, DataTable } from '@/components/ui';
import type { Column } from '@/components/ui';
import { useTranslation } from 'react-i18next';
import {
    Search,
    RefreshCw,
    Download
} from 'lucide-react';

interface StockLevel {
    id: string;
    productId: string;
    productName: string;
    productSku: string;
    warehouseId: string;
    warehouseName: string;
    quantityOnHand: number;
    quantityReserved: number;
    quantityOnOrder: number;
    quantityAvailable: number;
    lastUpdated: string;
}

export default function StockLevelsPage() {
    const { t } = useTranslation();
    const { error: toastError } = useToast();
    const [loading, setLoading] = useState(false);
    const [levels, setLevels] = useState<StockLevel[]>([]);

    // Filters
    const [warehouses, setWarehouses] = useState<any[]>([]);
    const [selectedWarehouse, setSelectedWarehouse] = useState<string>('');
    const [searchTerm, setSearchTerm] = useState('');

    useEffect(() => {
        loadWarehouses();
        loadStockLevels();
    }, []);

    useEffect(() => {
        loadStockLevels();
    }, [selectedWarehouse]);

    const loadWarehouses = async () => {
        const response = await api.warehouses.getAll();
        if (response.success && response.data) {
            setWarehouses(response.data);
        }
    };

    const loadStockLevels = async () => {
        setLoading(true);
        try {
            const response = await api.stock.getLevels(selectedWarehouse || undefined);
            if (response.success && response.data) {
                setLevels(response.data);
            } else {
                toastError(response.error || 'Failed to load stock levels');
            }
        } catch (err) {
            toastError('An unexpected error occurred');
        } finally {
            setLoading(false);
        }
    };

    const filteredLevels = levels.filter(level =>
        level.productName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        level.productSku.toLowerCase().includes(searchTerm.toLowerCase())
    );

    const columns: Column<StockLevel>[] = [
        {
            key: 'product',
            header: t('inventory.product', 'Product'),
            render: (level) => (
                <div>
                    <div className="text-sm font-medium text-gray-900">{level.productName}</div>
                    <div className="text-sm text-gray-500">{level.productSku}</div>
                </div>
            )
        },
        {
            key: 'warehouse',
            header: t('inventory.warehouse', 'Warehouse'),
            render: (level) => (
                <span className="text-sm text-gray-500">{level.warehouseName}</span>
            )
        },
        {
            key: 'onHand',
            header: t('inventory.onHand', 'On Hand'),
            render: (level) => (
                <div className="text-right font-medium">{level.quantityOnHand}</div>
            )
        },
        {
            key: 'reserved',
            header: t('inventory.reserved', 'Reserved'),
            render: (level) => (
                <div className="text-right text-gray-500">{level.quantityReserved}</div>
            )
        },
        {
            key: 'available',
            header: t('inventory.available', 'Available'),
            render: (level) => (
                <div className="text-right text-green-600 font-medium">{level.quantityAvailable}</div>
            )
        },
        {
            key: 'onOrder',
            header: t('inventory.onOrder', 'On Order'),
            render: (level) => (
                <div className="text-right text-blue-600">{level.quantityOnOrder}</div>
            )
        }
    ];

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">{t('inventory.stockLevels', 'Stock Levels')}</h1>
                    <p className="mt-1 text-sm text-gray-500">
                        {t('inventory.stockLevelsDesc', 'View current stock quantities across all warehouses')}
                    </p>
                </div>
                <div className="flex space-x-3">
                    <Button variant="secondary" onClick={loadStockLevels}>
                        <RefreshCw className="w-4 h-4 mr-2" />
                        {t('common.refresh', 'Refresh')}
                    </Button>
                    <Button variant="secondary">
                        <Download className="w-4 h-4 mr-2" />
                        {t('common.export', 'Export')}
                    </Button>
                </div>
            </div>

            {/* Filters */}
            <div className="bg-white p-4 rounded-lg shadow-sm border border-gray-200 flex flex-wrap gap-4 items-center">
                <div className="flex-1 min-w-[300px] relative">
                    <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none z-10">
                        <Search className="h-5 w-5 text-gray-400" />
                    </div>
                    <Input
                        placeholder={t('common.searchPlaceholder', 'Search by product name or SKU...')}
                        className="pl-10"
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                    />
                </div>

                <div className="w-64">
                    <Select
                        value={selectedWarehouse}
                        onChange={(e) => setSelectedWarehouse(e.target.value)}
                        options={[
                            { value: '', label: t('inventory.allWarehouses', 'All Warehouses') },
                            ...warehouses.map(w => ({ value: w.id, label: w.name }))
                        ]}
                    />
                </div>
            </div>

            {/* Table */}
            <DataTable
                data={filteredLevels}
                columns={columns}
                keyField="id"
                isLoading={loading}
                pagination={{
                    page: 1,
                    pageSize: 1000,
                    total: filteredLevels.length,
                    onPageChange: () => { }
                }}
            />
        </div>
    );
}
