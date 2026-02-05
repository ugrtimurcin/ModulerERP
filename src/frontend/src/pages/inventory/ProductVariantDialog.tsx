
import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, Input, Modal, useToast } from '@/components/ui';
import { api } from '@/services/api';
import { Plus, Trash2 } from 'lucide-react';

interface ProductVariantDialogProps {
    open: boolean;
    onClose: (refresh?: boolean) => void;
    variantId?: string | null;
    productId: string;
}

interface Attribute {
    key: string;
    value: string;
}

export function ProductVariantDialog({ open, onClose, variantId, productId }: ProductVariantDialogProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [isLoading, setIsLoading] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const [formData, setFormData] = useState({
        code: '',
        name: '',
    });

    // Attributes state (array for UI)
    const [attributes, setAttributes] = useState<Attribute[]>([]);

    useEffect(() => {
        if (open) {
            if (variantId) {
                loadVariant();
            } else {
                setFormData({ code: '', name: '' });
                setAttributes([]);
            }
        }
    }, [open, variantId]);

    const loadVariant = async () => {
        setIsLoading(true);
        try {
            const result = await api.productVariants.getById(variantId!);
            if (result.success && result.data) {
                const v = result.data;
                setFormData({
                    code: v.code,
                    name: v.name,
                });

                // Parse attributes JSON
                try {
                    const parsed = JSON.parse(v.attributes || '{}');
                    const attrs = Object.entries(parsed).map(([key, value]) => ({
                        key,
                        value: String(value)
                    }));
                    setAttributes(attrs);
                } catch {
                    setAttributes([]);
                }
            } else {
                toast.error(t('common.error'), result.error);
                onClose();
            }
        } catch {
            toast.error(t('common.error'));
            onClose();
        } finally {
            setIsLoading(false);
        }
    };

    const handleAddAttribute = () => {
        setAttributes([...attributes, { key: '', value: '' }]);
    };

    const handleRemoveAttribute = (index: number) => {
        const newAttrs = [...attributes];
        newAttrs.splice(index, 1);
        setAttributes(newAttrs);
    };

    const handleAttributeChange = (index: number, field: 'key' | 'value', val: string) => {
        const newAttrs = [...attributes];
        newAttrs[index][field] = val;
        setAttributes(newAttrs);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        // Validate attributes
        const attrObj: Record<string, string> = {};
        for (const a of attributes) {
            if (a.key.trim() && a.value.trim()) {
                attrObj[a.key.trim()] = a.value.trim();
            }
        }

        setIsSubmitting(true);
        try {
            if (variantId) {
                const payload = {
                    name: formData.name,
                    attributes: JSON.stringify(attrObj)
                };
                const res = await api.productVariants.update(variantId, payload);
                if (res.success) {
                    toast.success(t('common.saved'));
                    onClose(true);
                } else {
                    toast.error(t('common.error'), res.error);
                }
            } else {
                const payload = {
                    productId,
                    code: formData.code,
                    name: formData.name,
                    attributes: JSON.stringify(attrObj)
                };
                const res = await api.productVariants.create(payload);
                if (res.success) {
                    toast.success(t('common.created'));
                    onClose(true);
                } else {
                    toast.error(t('common.error'), res.error);
                }
            }
        } catch (err: any) {
            toast.error(t('common.error'), err.message);
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <Modal
            isOpen={open}
            onClose={() => onClose()}
            title={variantId ? t('inventory.editVariant') : t('inventory.createVariant')}
            size="md"
        >
            {isLoading ? (
                <div className="flex justify-center p-8"><div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div></div>
            ) : (
                <form onSubmit={handleSubmit} className="space-y-4">
                    <Input
                        label={t('inventory.code')}
                        value={formData.code}
                        onChange={e => setFormData({ ...formData, code: e.target.value })}
                        disabled={!!variantId} // Code is unique identifier usually immutable or harder to change
                        required
                    />
                    <Input
                        label={t('common.name')}
                        value={formData.name}
                        onChange={e => setFormData({ ...formData, name: e.target.value })}
                        required
                    />

                    <div className="space-y-2">
                        <div className="flex items-center justify-between">
                            <label className="text-sm font-medium">{t('inventory.attributes')}</label>
                            <Button type="button" variant="outline" size="sm" onClick={handleAddAttribute}>
                                <Plus className="w-3 h-3 mr-1" /> {t('common.add')}
                            </Button>
                        </div>

                        <div className="space-y-2 max-h-[200px] overflow-y-auto p-1">
                            {attributes.length === 0 && (
                                <p className="text-sm text-muted-foreground text-center py-2 italic">{t('inventory.noAttributes')}</p>
                            )}
                            {attributes.map((attr, index) => (
                                <div key={index} className="flex gap-2 items-center">
                                    <Input
                                        placeholder="Color, Size..."
                                        value={attr.key}
                                        onChange={e => handleAttributeChange(index, 'key', e.target.value)}
                                        className="flex-1"
                                    />
                                    <span className="text-muted-foreground">:</span>
                                    <Input
                                        placeholder="Red, XL..."
                                        value={attr.value}
                                        onChange={e => handleAttributeChange(index, 'value', e.target.value)}
                                        className="flex-1"
                                    />
                                    <Button type="button" variant="ghost" size="sm" onClick={() => handleRemoveAttribute(index)} className="text-red-600">
                                        <Trash2 className="w-4 h-4" />
                                    </Button>
                                </div>
                            ))}
                        </div>
                    </div>

                    <div className="flex justify-end gap-2 pt-4">
                        <Button type="button" variant="ghost" onClick={() => onClose()}>{t('common.cancel')}</Button>
                        <Button type="submit" isLoading={isSubmitting}>{t('common.save')}</Button>
                    </div>
                </form>
            )}
        </Modal>
    );
}
