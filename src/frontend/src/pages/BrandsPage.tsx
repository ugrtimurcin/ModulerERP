import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, Tag, Globe } from 'lucide-react';
import { api } from '@/services/api';
import { DataTable, Button, Modal, Input, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';

interface Brand {
    id: string;
    code: string;
    name: string;
    description?: string;
    website?: string;
    logoUrl?: string;
    isActive: boolean;
}

export function BrandsPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [brands, setBrands] = useState<Brand[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingBrand, setEditingBrand] = useState<Brand | null>(null);
    const [formData, setFormData] = useState({
        code: '',
        name: '',
        description: '',
        website: '',
    });

    const fetchBrands = useCallback(async () => {
        setIsLoading(true);
        const result = await api.brands.getAll();
        if (result.success && result.data) {
            setBrands(result.data);
        }
        setIsLoading(false);
    }, []);

    useEffect(() => {
        fetchBrands();
    }, [fetchBrands]);

    const openCreateModal = () => {
        setEditingBrand(null);
        setFormData({ code: '', name: '', description: '', website: '' });
        setIsModalOpen(true);
    };

    const openEditModal = (brand: Brand) => {
        setEditingBrand(brand);
        setFormData({
            code: brand.code,
            name: brand.name,
            description: brand.description || '',
            website: brand.website || '',
        });
        setIsModalOpen(true);
    };

    const handleSubmit = async () => {
        if (!formData.code.trim() || !formData.name.trim()) {
            toast.error(t('common.error'), t('common.required'));
            return;
        }

        if (editingBrand) {
            const result = await api.brands.update(editingBrand.id, {
                name: formData.name,
                description: formData.description || undefined,
                website: formData.website || undefined,
            });
            if (result.success) {
                toast.success(t('inventory.brands.updated'));
                setIsModalOpen(false);
                fetchBrands();
            } else {
                toast.error(t('common.error'), result.error);
            }
        } else {
            const result = await api.brands.create({
                code: formData.code,
                name: formData.name,
                description: formData.description || undefined,
                website: formData.website || undefined,
            });
            if (result.success) {
                toast.success(t('inventory.brands.created'));
                setIsModalOpen(false);
                fetchBrands();
            } else {
                toast.error(t('common.error'), result.error);
            }
        }
    };

    const handleDelete = async (brand: Brand) => {
        const confirmed = await dialog.danger({
            title: t('common.confirmDelete'),
            message: t('inventory.brands.deleteConfirm', { name: brand.name }),
            confirmText: t('common.delete'),
        });
        if (confirmed) {
            const result = await api.brands.delete(brand.id);
            if (result.success) {
                toast.success(t('inventory.brands.deleted'));
                fetchBrands();
            }
        }
    };

    const columns: Column<Brand>[] = [
        { key: 'code', header: t('common.code') },
        { key: 'name', header: t('common.name') },
        { key: 'description', header: t('common.description') },
        {
            key: 'website',
            header: t('inventory.brands.website'),
            render: (brand) => brand.website ? (
                <a href={brand.website} target="_blank" rel="noopener noreferrer" className="text-blue-600 hover:underline flex items-center gap-1">
                    <Globe className="w-3 h-3" />
                    {brand.website}
                </a>
            ) : '-',
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <Tag className="w-7 h-7" />
                        {t('inventory.brands.title')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('inventory.brands.subtitle')}</p>
                </div>
                <Button onClick={openCreateModal}>
                    <Plus className="w-4 h-4" />
                    {t('inventory.brands.create')}
                </Button>
            </div>

            <DataTable
                data={brands}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                actions={(brand) => (
                    <div className="flex gap-2">
                        <button onClick={() => openEditModal(brand)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]">
                            <Pencil className="w-4 h-4" />
                        </button>
                        <button onClick={() => handleDelete(brand)} className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600">
                            <Trash2 className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            <Modal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title={editingBrand ? t('inventory.brands.edit') : t('inventory.brands.create')}
                size="md"
                footer={
                    <>
                        <Button variant="secondary" onClick={() => setIsModalOpen(false)}>{t('common.cancel')}</Button>
                        <Button onClick={handleSubmit}>{t('common.save')}</Button>
                    </>
                }
            >
                <div className="space-y-4">
                    <Input
                        label={t('common.code')}
                        value={formData.code}
                        onChange={(e) => setFormData({ ...formData, code: e.target.value })}
                        disabled={!!editingBrand}
                        required
                    />
                    <Input
                        label={t('common.name')}
                        value={formData.name}
                        onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                        required
                    />
                    <Input
                        label={t('common.description')}
                        value={formData.description}
                        onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    />
                    <Input
                        label={t('inventory.brands.website')}
                        value={formData.website}
                        onChange={(e) => setFormData({ ...formData, website: e.target.value })}
                        placeholder="https://..."
                    />
                </div>
            </Modal>
        </div>
    );
}
