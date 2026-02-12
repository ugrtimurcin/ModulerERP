import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { Plus, Pencil, Trash2, Building2, Package, UserCheck } from 'lucide-react';
import { DataTable, Button, Badge, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';
import { api } from '@/lib/api';
import { AssetDialog } from './AssetDialog';

interface Asset {
    id: string;
    assetCode: string;
    name: string;
    description: string | null;
    categoryId: string;
    categoryName: string;
    status: number;
    acquisitionDate: string;
    acquisitionCost: number;
    salvageValue: number;
    accumulatedDepreciation: number;
    bookValue: number;
    locationId: string | null;
    departmentId: string | null;
    assignedEmployeeId: string | null;
    assignedEmployeeName: string | null;
    serialNumber: string | null;
    barCode: string | null;
}

interface AssetCategory {
    id: string;
    name: string;
    code: string;
}

const AssetStatus: Record<number, { label: string; variant: 'success' | 'warning' | 'error' | 'default' }> = {
    0: { label: 'In Stock', variant: 'default' },
    1: { label: 'Assigned', variant: 'success' },
    2: { label: 'Under Maintenance', variant: 'warning' },
    3: { label: 'Disposed', variant: 'error' },
    4: { label: 'Scrapped', variant: 'error' },
    5: { label: 'Sold', variant: 'default' },
};

export function AssetsPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();
    const navigate = useNavigate();

    const [assets, setAssets] = useState<Asset[]>([]);
    const [categories, setCategories] = useState<AssetCategory[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [dialogOpen, setDialogOpen] = useState(false);
    const [editingAsset, setEditingAsset] = useState<Asset | null>(null);

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const [assetsRes, categoriesRes] = await Promise.all([
                api.get<any>('/fixedassets/assets'),
                api.get<any>('/fixedassets/categories')
            ]);

            setAssets(assetsRes.data || assetsRes);
            setCategories(categoriesRes.data || categoriesRes);
        } catch (error) {
            toast.error(t('common.error'));
        }
        setIsLoading(false);
    }, [toast, t]);

    useEffect(() => {
        loadData();
    }, [loadData]);

    const handleCreate = () => {
        setEditingAsset(null);
        setDialogOpen(true);
    };

    const handleEdit = (asset: Asset) => {
        setEditingAsset(asset);
        setDialogOpen(true);
    };

    const handleDelete = async (asset: Asset) => {
        const confirmed = await dialog.danger({
            title: t('fixedAssets.deleteAsset'),
            message: t('fixedAssets.confirmDeleteAsset', { name: asset.name }),
            confirmText: t('common.delete'),
        });

        if (confirmed) {
            try {
                await api.delete(`/fixedassets/assets/${asset.id}`);
                toast.success(t('fixedAssets.assetDeleted'));
                loadData();
            } catch {
                toast.error(t('common.error'));
            }
        }
    };

    const handleDialogClose = (saved: boolean) => {
        setDialogOpen(false);
        if (saved) loadData();
    };

    const getStatusBadge = (status: number) => {
        const config = AssetStatus[status] || { label: 'Unknown', variant: 'default' as const };
        return <Badge variant={config.variant}>{t(`fixedAssets.status.${status}`) || config.label}</Badge>;
    };

    const columns: Column<Asset>[] = [
        {
            key: 'asset',
            header: t('fixedAssets.asset'),
            render: (asset) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-amber-500 to-orange-500 flex items-center justify-center text-white">
                        <Package className="w-5 h-5" />
                    </div>
                    <div>
                        <p className="font-medium">{asset.name}</p>
                        <p className="text-xs text-[hsl(var(--muted-foreground))] font-mono">{asset.assetCode}</p>
                    </div>
                </div>
            ),
        },
        {
            key: 'category',
            header: t('fixedAssets.category'),
            render: (asset) => (
                <span className="text-sm">{asset.categoryName}</span>
            ),
        },
        {
            key: 'acquisitionCost',
            header: t('fixedAssets.acquisitionCost'),
            render: (asset) => (
                <span className="font-mono font-medium">
                    {asset.acquisitionCost?.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                </span>
            ),
        },
        {
            key: 'bookValue',
            header: t('fixedAssets.bookValue'),
            render: (asset) => (
                <span className="font-mono font-medium text-indigo-600 dark:text-indigo-400">
                    {asset.bookValue?.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                </span>
            ),
        },
        {
            key: 'assignee',
            header: t('fixedAssets.assignedTo'),
            render: (asset) => (
                asset.assignedEmployeeName ? (
                    <div className="flex items-center gap-2">
                        <UserCheck className="w-4 h-4 text-green-500" />
                        <span>{asset.assignedEmployeeName}</span>
                    </div>
                ) : (
                    <span className="text-[hsl(var(--muted-foreground))]">—</span>
                )
            ),
        },
        {
            key: 'status',
            header: t('common.status'),
            render: (asset) => getStatusBadge(asset.status),
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <Building2 className="w-6 h-6" />
                        {t('fixedAssets.assets')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">
                        {t('fixedAssets.assetsSubtitle')}
                    </p>
                </div>
                <Button onClick={handleCreate}>
                    <Plus className="w-4 h-4" />
                    {t('fixedAssets.createAsset')}
                </Button>
            </div>

            <DataTable
                data={assets}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                actions={(asset) => (
                    <div className="flex items-center gap-1">
                        <button
                            onClick={() => navigate(`/fixed-assets/assets/${asset.id}`)}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                            title={t('common.view')}
                        >
                            <Package className="w-4 h-4" />
                        </button>
                        <button
                            onClick={() => handleEdit(asset)}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                            title={t('common.edit')}
                        >
                            <Pencil className="w-4 h-4" />
                        </button>
                        <button
                            onClick={() => handleDelete(asset)}
                            className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600 transition-colors"
                            title={t('common.delete')}
                        >
                            <Trash2 className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            <AssetDialog
                open={dialogOpen}
                onClose={handleDialogClose}
                asset={editingAsset}
                categories={categories}
            />
        </div>
    );
}
