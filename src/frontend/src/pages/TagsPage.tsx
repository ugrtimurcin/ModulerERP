import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, Tag } from 'lucide-react';
import { api } from '@/services/api';
import { DataTable, Button, Modal, Input, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';

interface TagItem {
    id: string;
    name: string;
    colorCode: string;
    entityType: string | null;
}

interface TagFormData {
    name: string;
    colorCode: string;
    entityType: string;
}

const PRESET_COLORS = [
    '#EF4444', '#F97316', '#F59E0B', '#84CC16',
    '#22C55E', '#14B8A6', '#06B6D4', '#3B82F6',
    '#6366F1', '#8B5CF6', '#A855F7', '#EC4899'
];

const ENTITY_TYPES = [
    { value: '', label: 'All Entities' },
    { value: 'Partner', label: 'Partners' },
    { value: 'Lead', label: 'Leads' },
    { value: 'Ticket', label: 'Tickets' },
    { value: 'Opportunity', label: 'Opportunities' },
];

export function TagsPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [tags, setTags] = useState<TagItem[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    // Modal state
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingTag, setEditingTag] = useState<TagItem | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Form state
    const [formData, setFormData] = useState<TagFormData>({
        name: '',
        colorCode: '#3B82F6',
        entityType: '',
    });
    const [formErrors, setFormErrors] = useState<Partial<TagFormData>>({});

    const loadTags = useCallback(async () => {
        setIsLoading(true);
        const result = await api.tags.getAll();
        if (result.success && result.data) {
            setTags(result.data);
        }
        setIsLoading(false);
    }, []);

    useEffect(() => {
        loadTags();
    }, [loadTags]);

    const openCreateModal = () => {
        setEditingTag(null);
        setFormData({
            name: '',
            colorCode: '#3B82F6',
            entityType: '',
        });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const openEditModal = (tag: TagItem) => {
        setEditingTag(tag);
        setFormData({
            name: tag.name,
            colorCode: tag.colorCode,
            entityType: tag.entityType || '',
        });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const validateForm = (): boolean => {
        const errors: Partial<TagFormData> = {};

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
                name: formData.name,
                colorCode: formData.colorCode,
                entityType: formData.entityType || undefined,
            };

            if (editingTag) {
                const result = await api.tags.update(editingTag.id, payload);
                if (result.success) {
                    toast.success(t('tags.tagUpdated'));
                    setIsModalOpen(false);
                    loadTags();
                } else {
                    toast.error(t('common.error'), result.error);
                }
            } else {
                const result = await api.tags.create(payload);
                if (result.success) {
                    toast.success(t('tags.tagCreated'));
                    setIsModalOpen(false);
                    loadTags();
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

    const handleDelete = async (tag: TagItem) => {
        const confirmed = await dialog.danger({
            title: t('tags.deleteTag'),
            message: t('tags.confirmDeleteTag'),
            confirmText: t('common.delete'),
        });

        if (confirmed) {
            const result = await api.tags.delete(tag.id);
            if (result.success) {
                toast.success(t('tags.tagDeleted'));
                loadTags();
            } else {
                toast.error(t('common.error'), result.error);
            }
        }
    };

    const columns: Column<TagItem>[] = [
        {
            key: 'tag',
            header: t('tags.name'),
            render: (tag) => (
                <div className="flex items-center gap-3">
                    <div
                        className="w-8 h-8 rounded-lg flex items-center justify-center"
                        style={{ backgroundColor: tag.colorCode + '20' }}
                    >
                        <Tag className="w-4 h-4" style={{ color: tag.colorCode }} />
                    </div>
                    <div>
                        <p className="font-medium">{tag.name}</p>
                        <span
                            className="inline-block px-2 py-0.5 rounded text-xs text-white"
                            style={{ backgroundColor: tag.colorCode }}
                        >
                            {tag.colorCode}
                        </span>
                    </div>
                </div>
            ),
        },
        {
            key: 'entityType',
            header: t('tags.entityType'),
            render: (tag) => (
                <span className="text-sm text-[hsl(var(--muted-foreground))]">
                    {tag.entityType || 'All Entities'}
                </span>
            ),
        },
    ];

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">{t('tags.title')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">
                        {t('tags.subtitle')}
                    </p>
                </div>
                <Button onClick={openCreateModal}>
                    <Plus className="w-4 h-4" />
                    {t('tags.createTag')}
                </Button>
            </div>

            {/* Table */}
            <DataTable
                data={tags}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                actions={(tag) => (
                    <div className="flex items-center gap-1">
                        <button
                            onClick={() => openEditModal(tag)}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                            title={t('common.edit')}
                        >
                            <Pencil className="w-4 h-4" />
                        </button>
                        <button
                            onClick={() => handleDelete(tag)}
                            className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600 transition-colors"
                            title={t('common.delete')}
                        >
                            <Trash2 className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            {/* Create/Edit Modal */}
            <Modal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title={editingTag ? t('tags.editTag') : t('tags.createTag')}
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
                        label={t('tags.name')}
                        value={formData.name}
                        onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                        error={formErrors.name}
                        required
                    />

                    <div>
                        <label className="block text-sm font-medium mb-2">{t('tags.color')}</label>
                        <div className="flex flex-wrap gap-2">
                            {PRESET_COLORS.map((color) => (
                                <button
                                    key={color}
                                    type="button"
                                    onClick={() => setFormData({ ...formData, colorCode: color })}
                                    className={`w-8 h-8 rounded-lg border-2 transition-all ${formData.colorCode === color
                                            ? 'border-[hsl(var(--foreground))] scale-110'
                                            : 'border-transparent hover:scale-105'
                                        }`}
                                    style={{ backgroundColor: color }}
                                />
                            ))}
                        </div>
                        <div className="flex items-center gap-2 mt-3">
                            <input
                                type="color"
                                value={formData.colorCode}
                                onChange={(e) => setFormData({ ...formData, colorCode: e.target.value })}
                                className="w-10 h-10 rounded cursor-pointer"
                            />
                            <Input
                                value={formData.colorCode}
                                onChange={(e) => setFormData({ ...formData, colorCode: e.target.value })}
                                placeholder="#000000"
                                className="flex-1"
                            />
                        </div>
                    </div>

                    <div>
                        <label className="block text-sm font-medium mb-2">{t('tags.entityType')}</label>
                        <select
                            value={formData.entityType}
                            onChange={(e) => setFormData({ ...formData, entityType: e.target.value })}
                            className="w-full px-3 py-2 border border-[hsl(var(--border))] rounded-lg bg-[hsl(var(--background))]"
                        >
                            {ENTITY_TYPES.map((type) => (
                                <option key={type.value} value={type.value}>
                                    {type.label}
                                </option>
                            ))}
                        </select>
                        <p className="text-xs text-[hsl(var(--muted-foreground))] mt-1">
                            {t('tags.entityTypeHint')}
                        </p>
                    </div>
                </div>
            </Modal>
        </div>
    );
}
