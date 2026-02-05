import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, FolderTree } from 'lucide-react';
import { DataTable, Button, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';
import { AssetCategoryDialog } from './AssetCategoryDialog';

interface AssetCategory {
    id: string;
    code: string;
    name: string;
    description: string | null;
    depreciationMethod: number;
    usefulLifeMonths: number;
}

const API_BASE = '/api/fixedassets';

const DepreciationMethods: Record<number, string> = {
    0: 'Straight Line',
    1: 'Declining Balance',
    2: 'Sum of Years',
    3: 'Units of Production',
};

export function AssetCategoriesPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [categories, setCategories] = useState<AssetCategory[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [dialogOpen, setDialogOpen] = useState(false);
    const [editingCategory, setEditingCategory] = useState<AssetCategory | null>(null);

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const res = await fetch(`${API_BASE}/categories`, { cache: 'no-store' });
            if (res.ok) {
                const data = await res.json();
                setCategories(data.data || data);
            }
        } catch (error) {
            toast.error(t('common.error'));
        }
        setIsLoading(false);
    }, [toast, t]);

    useEffect(() => {
        loadData();
    }, [loadData]);

    const handleCreate = () => {
        setEditingCategory(null);
        setDialogOpen(true);
    };

    const handleEdit = (category: AssetCategory) => {
        setEditingCategory(category);
        setDialogOpen(true);
    };

    const handleDelete = async (category: AssetCategory) => {
        const confirmed = await dialog.danger({
            title: t('fixedAssets.deleteCategory'),
            message: t('fixedAssets.confirmDeleteCategory', { name: category.name }),
            confirmText: t('common.delete'),
        });

        if (confirmed) {
            try {
                const res = await fetch(`${API_BASE}/categories/${category.id}`, { method: 'DELETE', cache: 'no-store' });
                if (res.ok) {
                    toast.success(t('fixedAssets.categoryDeleted'));
                    loadData();
                } else {
                    toast.error(t('common.error'));
                }
            } catch {
                toast.error(t('common.error'));
            }
        }
    };

    const handleDialogClose = (saved: boolean) => {
        setDialogOpen(false);
        if (saved) loadData();
    };

    const columns: Column<AssetCategory>[] = [
        {
            key: 'code',
            header: t('common.code'),
            render: (cat) => <span className="font-mono">{cat.code}</span>,
        },
        {
            key: 'name',
            header: t('common.name'),
            render: (cat) => (
                <div>
                    <p className="font-medium">{cat.name}</p>
                    {cat.description && (
                        <p className="text-xs text-[hsl(var(--muted-foreground))]">{cat.description}</p>
                    )}
                </div>
            ),
        },
        {
            key: 'depreciationMethod',
            header: t('fixedAssets.depreciationMethod'),
            render: (cat) => DepreciationMethods[cat.depreciationMethod] || 'Unknown',
        },
        {
            key: 'usefulLifeMonths',
            header: t('fixedAssets.usefulLife'),
            render: (cat) => `${cat.usefulLifeMonths} ${t('fixedAssets.months')}`,
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <FolderTree className="w-6 h-6" />
                        {t('fixedAssets.categories')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">
                        {t('fixedAssets.categoriesSubtitle')}
                    </p>
                </div>
                <Button onClick={handleCreate}>
                    <Plus className="w-4 h-4" />
                    {t('fixedAssets.createCategory')}
                </Button>
            </div>

            <DataTable
                data={categories}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                actions={(category) => (
                    <div className="flex items-center gap-1">
                        <button
                            onClick={() => handleEdit(category)}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                            title={t('common.edit')}
                        >
                            <Pencil className="w-4 h-4" />
                        </button>
                        <button
                            onClick={() => handleDelete(category)}
                            className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600 transition-colors"
                            title={t('common.delete')}
                        >
                            <Trash2 className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            <AssetCategoryDialog
                open={dialogOpen}
                onClose={handleDialogClose}
                category={editingCategory}
            />
        </div>
    );
}
