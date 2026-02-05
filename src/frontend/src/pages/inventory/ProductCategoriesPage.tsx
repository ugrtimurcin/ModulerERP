import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, FolderTree } from 'lucide-react';
import { api } from '@/services/api';
import { DataTable, Button, Modal, Input, Badge, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';

interface ProductCategory {
    id: string;
    name: string;
    description: string | null;
    parentCategoryId: string | null;
    parentCategoryName: string | null;
    sortOrder: number;
    isActive: boolean;
}

interface CategoryFormData {
    name: string;
    description: string;
    parentCategoryId: string;
    sortOrder: number;
    isActive: boolean;
}

export function ProductCategoriesPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [categories, setCategories] = useState<ProductCategory[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    // Modal state
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingCategory, setEditingCategory] = useState<ProductCategory | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Form state
    const [formData, setFormData] = useState<CategoryFormData>({
        name: '',
        description: '',
        parentCategoryId: '',
        sortOrder: 0,
        isActive: true,
    });
    const [formErrors, setFormErrors] = useState<Partial<CategoryFormData>>({});

    const loadCategories = useCallback(async () => {
        setIsLoading(true);
        const result = await api.productCategories.getAll();
        if (result.success && result.data) {
            setCategories(result.data);
        }
        setIsLoading(false);
    }, []);

    useEffect(() => {
        loadCategories();
    }, [loadCategories]);

    const openCreateModal = () => {
        setEditingCategory(null);
        setFormData({
            name: '',
            description: '',
            parentCategoryId: '',
            sortOrder: 0,
            isActive: true,
        });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const openEditModal = (category: ProductCategory) => {
        setEditingCategory(category);
        setFormData({
            name: category.name,
            description: category.description || '',
            parentCategoryId: category.parentCategoryId || '',
            sortOrder: category.sortOrder,
            isActive: category.isActive,
        });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const validateForm = (): boolean => {
        const errors: Partial<CategoryFormData> = {};
        if (!formData.name.trim()) {
            errors.name = t('common.required');
        }
        setFormErrors(errors);
        return Object.keys(errors).length === 0;
    };

    const handleSubmit = async () => {
        if (!validateForm()) return;

        setIsSubmitting(true);
        try {
            const payload = {
                ...formData,
                parentCategoryId: formData.parentCategoryId || null,
            };

            if (editingCategory) {
                const result = await api.productCategories.update(editingCategory.id, payload);
                if (result.success) {
                    toast.success(t('inventory.categoryUpdated'));
                    setIsModalOpen(false);
                    loadCategories();
                } else {
                    toast.error(t('common.error'), result.error);
                }
            } else {
                const result = await api.productCategories.create(payload);
                if (result.success) {
                    toast.success(t('inventory.categoryCreated'));
                    setIsModalOpen(false);
                    loadCategories();
                } else {
                    toast.error(t('common.error'), result.error);
                }
            }
        } catch (err) {
            toast.error(t('common.error'), (err as Error).message);
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleDelete = async (category: ProductCategory) => {
        const confirmed = await dialog.danger({
            title: t('inventory.deleteCategory'),
            message: t('inventory.confirmDeleteCategory'),
            confirmText: t('common.delete'),
        });

        if (confirmed) {
            const result = await api.productCategories.delete(category.id);
            if (result.success) {
                toast.success(t('inventory.categoryDeleted'));
                loadCategories();
            } else {
                toast.error(t('common.error'), result.error);
            }
        }
    };

    const columns: Column<ProductCategory>[] = [
        {
            key: 'name',
            header: t('inventory.categoryName'),
            render: (category) => (
                <div className="flex items-center gap-3">
                    <div className="w-8 h-8 rounded bg-blue-100 dark:bg-blue-900/30 text-blue-600 flex items-center justify-center">
                        <FolderTree className="w-4 h-4" />
                    </div>
                    <div>
                        <p className="font-medium">{category.name}</p>
                        {category.description && (
                            <p className="text-xs text-[hsl(var(--muted-foreground))]">{category.description}</p>
                        )}
                    </div>
                </div>
            ),
        },
        {
            key: 'parentCategoryName',
            header: t('inventory.parentCategory'),
            render: (category) => category.parentCategoryName || '-',
        },
        {
            key: 'sortOrder',
            header: t('inventory.sortOrder'),
        },
        {
            key: 'isActive',
            header: t('common.status'),
            render: (category) => (
                <Badge variant={category.isActive ? 'success' : 'default'}>
                    {category.isActive ? t('common.active') : t('common.inactive')}
                </Badge>
            ),
        },
    ];

    // Filter out current category and its children to prevent cycles (simplified check)
    const parentOptions = categories.filter(c => !editingCategory || c.id !== editingCategory.id);

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">{t('inventory.categories')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">
                        {t('inventory.categoriesSubtitle')}
                    </p>
                </div>
                <Button onClick={openCreateModal}>
                    <Plus className="w-4 h-4" />
                    {t('inventory.createCategory')}
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
                            onClick={() => openEditModal(category)}
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

            <Modal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title={editingCategory ? t('inventory.editCategory') : t('inventory.createCategory')}
                size="md"
                footer={
                    <>
                        <Button variant="secondary" onClick={() => setIsModalOpen(false)}>
                            {t('common.cancel')}
                        </Button>
                        <Button onClick={handleSubmit} isLoading={isSubmitting}>
                            {t('common.save')}
                        </Button>
                    </>
                }
            >
                <div className="space-y-4">
                    <Input
                        label={t('inventory.categoryName')}
                        value={formData.name}
                        onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                        error={formErrors.name}
                        required
                    />

                    <div className="space-y-2">
                        <label className="text-sm font-medium">{t('inventory.parentCategory')}</label>
                        <select
                            className="w-full h-10 px-3 py-2 rounded-md border border-[hsl(var(--input))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-[hsl(var(--primary))]"
                            value={formData.parentCategoryId}
                            onChange={(e) => setFormData({ ...formData, parentCategoryId: e.target.value })}
                        >
                            <option value="">{t('common.none')}</option>
                            {parentOptions.map(c => (
                                <option key={c.id} value={c.id}>{c.name}</option>
                            ))}
                        </select>
                    </div>

                    <Input
                        label={t('common.description')}
                        value={formData.description}
                        onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    />

                    <div className="grid grid-cols-2 gap-4">
                        <Input
                            label={t('inventory.sortOrder')}
                            type="number"
                            value={formData.sortOrder}
                            onChange={(e) => setFormData({ ...formData, sortOrder: parseInt(e.target.value) || 0 })}
                        />
                        <div className="flex items-center h-full pt-6">
                            <label className="flex items-center gap-2 cursor-pointer">
                                <input
                                    type="checkbox"
                                    checked={formData.isActive}
                                    onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                                    className="rounded border-[hsl(var(--border))]"
                                />
                                <span>{t('common.isActive')}</span>
                            </label>
                        </div>
                    </div>
                </div>
            </Modal>
        </div>
    );
}
