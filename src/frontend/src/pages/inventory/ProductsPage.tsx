import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { Plus, Pencil, Trash2, Package } from 'lucide-react';
import { api } from '@/services/api';
import { DataTable, Button, Badge, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';

interface Product {
    id: string;
    sku: string;
    name: string;
    type: number;
    unitOfMeasureCode: string;
    categoryName: string | null;
    salesPrice: number;
    isActive: boolean;
}

export function ProductsPage() {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const toast = useToast();
    const dialog = useDialog();

    const [products, setProducts] = useState<Product[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    const loadProducts = useCallback(async () => {
        setIsLoading(true);
        const result = await api.products.getAll();
        if (result.success && result.data) {
            setProducts(result.data);
        }
        setIsLoading(false);
    }, []);

    useEffect(() => {
        loadProducts();
    }, [loadProducts]);

    const handleDelete = async (product: Product) => {
        const confirmed = await dialog.danger({
            title: t('inventory.deleteProduct'),
            message: t('inventory.confirmDeleteProduct'),
            confirmText: t('common.delete'),
        });

        if (confirmed) {
            const result = await api.products.delete(product.id);
            if (result.success) {
                toast.success(t('inventory.productDeleted'));
                loadProducts();
            } else {
                toast.error(t('common.error'), result.error);
            }
        }
    };

    const columns: Column<Product>[] = [
        {
            key: 'sku',
            header: t('inventory.sku'),
            render: (product) => (
                <div className="flex items-center gap-3">
                    <div className="w-8 h-8 rounded bg-indigo-100 dark:bg-indigo-900/30 text-indigo-600 flex items-center justify-center">
                        <Package className="w-4 h-4" />
                    </div>
                    <div>
                        <span className="font-medium font-mono text-sm">{product.sku}</span>
                    </div>
                </div>
            ),
        },
        {
            key: 'name',
            header: t('common.name'), // Assuming common.name exists, or use inventory.productName if I created it. I used generic Name in other places.
            render: (product) => (
                <div>
                    <p className="font-medium">{product.name}</p>
                    {product.categoryName && (
                        <span className="text-xs text-[hsl(var(--muted-foreground))] flex items-center gap-1">
                            {product.categoryName}
                        </span>
                    )}
                </div>
            )
        },
        {
            key: 'type',
            header: t('inventory.type'),
            render: (product) => {
                return <span className="text-sm">{t(`inventory.productTypes.${product.type}`)}</span>;
            }
        },
        {
            key: 'unitOfMeasureCode',
            header: t('inventory.uom'),
        },
        {
            key: 'salesPrice',
            header: t('inventory.salesPrice'),
            render: (product) => (
                <span className="font-mono">
                    {product.salesPrice?.toFixed(2)}
                </span>
            )
        },
        {
            key: 'isActive',
            header: t('common.status'),
            render: (product) => (
                <Badge variant={product.isActive ? 'success' : 'default'}>
                    {product.isActive ? t('common.active') : t('common.inactive')}
                </Badge>
            ),
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">{t('inventory.products')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">
                        {t('inventory.productsSubtitle')}
                    </p>
                </div>
                <Button onClick={() => navigate('/inventory/products/new')}>
                    <Plus className="w-4 h-4" />
                    {t('inventory.createProduct')}
                </Button>
            </div>

            <DataTable
                data={products}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                actions={(product) => (
                    <div className="flex items-center gap-1">
                        <button
                            onClick={() => navigate(`/inventory/products/${product.id}`)}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                            title={t('common.edit')}
                        >
                            <Pencil className="w-4 h-4" />
                        </button>
                        <button
                            onClick={() => handleDelete(product)}
                            className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600 transition-colors"
                            title={t('common.delete')}
                        >
                            <Trash2 className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />
        </div>
    );
}
