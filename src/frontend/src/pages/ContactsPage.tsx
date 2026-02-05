import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, User, Phone, Mail, Star } from 'lucide-react';
import { api } from '@/services/api';
import { DataTable, Button, Modal, Input, Badge, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';

interface Contact {
    id: string;
    partnerId: string;
    firstName: string;
    lastName: string;
    fullName: string;
    position: string | null;
    email: string | null;
    phone: string | null;
    isPrimary: boolean;
    isActive: boolean;
    createdAt: string;
}

interface Partner {
    id: string;
    name: string;
}

interface ContactFormData {
    partnerId: string;
    firstName: string;
    lastName: string;
    position: string;
    email: string;
    phone: string;
    isPrimary: boolean;
}

export function ContactsPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [contacts, setContacts] = useState<Contact[]>([]);
    const [partners, setPartners] = useState<Partner[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [total, setTotal] = useState(0);
    const pageSize = 20;

    // Modal state
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingContact, setEditingContact] = useState<Contact | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Form state
    const [formData, setFormData] = useState<ContactFormData>({
        partnerId: '',
        firstName: '',
        lastName: '',
        position: '',
        email: '',
        phone: '',
        isPrimary: false,
    });
    const [formErrors, setFormErrors] = useState<Partial<ContactFormData>>({});

    const loadContacts = useCallback(async () => {
        setIsLoading(true);
        const result = await api.contacts.getAll(page, pageSize);
        if (result.success && result.data) {
            setContacts(result.data.data);
            setTotal(result.data.totalCount);
        }
        setIsLoading(false);
    }, [page]);

    const loadPartners = async () => {
        const result = await api.partners.getAll(1, 1000); // Load all for dropdown
        if (result.success && result.data) {
            setPartners(result.data.data.map((p: any) => ({ id: p.id, name: p.name })));
        }
    };

    useEffect(() => {
        loadContacts();
        loadPartners();
    }, [loadContacts]);

    const openCreateModal = () => {
        setEditingContact(null);
        setFormData({
            partnerId: '',
            firstName: '',
            lastName: '',
            position: '',
            email: '',
            phone: '',
            isPrimary: false,
        });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const openEditModal = (contact: Contact) => {
        setEditingContact(contact);
        setFormData({
            partnerId: contact.partnerId,
            firstName: contact.firstName,
            lastName: contact.lastName,
            position: contact.position || '',
            email: contact.email || '',
            phone: contact.phone || '',
            isPrimary: contact.isPrimary,
        });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const validateForm = (): boolean => {
        const errors: Partial<ContactFormData> = {};

        if (!formData.partnerId) {
            errors.partnerId = t('common.required');
        }
        if (!formData.firstName.trim()) {
            errors.firstName = t('common.required');
        }
        if (!formData.lastName.trim()) {
            errors.lastName = t('common.required');
        }

        setFormErrors(errors);
        return Object.keys(errors).length === 0;
    };

    const handleSubmit = async () => {
        if (!validateForm()) return;

        setIsSubmitting(true);
        try {
            if (editingContact) {
                const result = await api.contacts.update(editingContact.id, formData);
                if (result.success) {
                    toast.success(t('contacts.contactUpdated'));
                    setIsModalOpen(false);
                    loadContacts();
                } else {
                    toast.error(t('common.error'), result.error);
                }
            } else {
                const result = await api.contacts.create(formData);
                if (result.success) {
                    toast.success(t('contacts.contactCreated'));
                    setIsModalOpen(false);
                    loadContacts();
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

    const handleDelete = async (contact: Contact) => {
        const confirmed = await dialog.danger({
            title: t('contacts.deleteContact'),
            message: t('contacts.confirmDeleteContact'),
            confirmText: t('common.delete'),
        });

        if (confirmed) {
            const result = await api.contacts.delete(contact.id);
            if (result.success) {
                toast.success(t('contacts.contactDeleted'));
                loadContacts();
            } else {
                toast.error(t('common.error'), result.error);
            }
        }
    };

    const columns: Column<Contact>[] = [
        {
            key: 'name',
            header: t('contacts.name'),
            render: (contact) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-full bg-indigo-100 dark:bg-indigo-900/30 text-indigo-600 flex items-center justify-center">
                        <User className="w-5 h-5" />
                    </div>
                    <div>
                        <div className="flex items-center gap-2">
                            <p className="font-medium">{contact.fullName}</p>
                            {contact.isPrimary && (
                                <Star className="w-3 h-3 text-yellow-500 fill-current" />
                            )}
                        </div>
                        <p className="text-sm text-[hsl(var(--muted-foreground))]">{contact.position}</p>
                    </div>
                </div>
            ),
        },
        {
            key: 'contact',
            header: t('contacts.contactInfo'),
            render: (contact) => (
                <div className="space-y-1 text-sm">
                    {contact.email && (
                        <div className="flex items-center gap-2 text-[hsl(var(--muted-foreground))]">
                            <Mail className="w-3 h-3" />
                            <span>{contact.email}</span>
                        </div>
                    )}
                    {contact.phone && (
                        <div className="flex items-center gap-2 text-[hsl(var(--muted-foreground))]">
                            <Phone className="w-3 h-3" />
                            <span>{contact.phone}</span>
                        </div>
                    )}
                </div>
            ),
        },
        {
            key: 'isActive',
            header: t('common.status'),
            render: (contact) => (
                <Badge variant={contact.isActive ? 'success' : 'error'}>
                    {contact.isActive ? t('common.active') : t('common.inactive')}
                </Badge>
            ),
        },
    ];

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">{t('contacts.title')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">
                        {t('contacts.subtitle')}
                    </p>
                </div>
                <Button onClick={openCreateModal}>
                    <Plus className="w-4 h-4" />
                    {t('contacts.createContact')}
                </Button>
            </div>

            {/* Table */}
            <DataTable
                data={contacts}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                pagination={{
                    page,
                    pageSize,
                    total,
                    onPageChange: setPage,
                }}
                actions={(contact) => (
                    <div className="flex items-center gap-1">
                        <button
                            onClick={() => openEditModal(contact)}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                            title={t('common.edit')}
                        >
                            <Pencil className="w-4 h-4" />
                        </button>
                        <button
                            onClick={() => handleDelete(contact)}
                            className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600 transition-colors"
                            title={t('common.delete')}
                        >
                            <Trash2 className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            {/* Modal */}
            <Modal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title={editingContact ? t('contacts.editContact') : t('contacts.createContact')}
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
                    <div className="space-y-2">
                        <label className="text-sm font-medium">{t('contacts.partner')}</label>
                        <select
                            className="w-full h-10 px-3 py-2 rounded-md border border-[hsl(var(--input))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-[hsl(var(--primary))]"
                            value={formData.partnerId}
                            onChange={(e) => setFormData({ ...formData, partnerId: e.target.value })}
                            disabled={!!editingContact} // Usually partner shouldn't change, but ok
                        >
                            <option value="">{t('common.select')}</option>
                            {partners.map(p => (
                                <option key={p.id} value={p.id}>{p.name}</option>
                            ))}
                        </select>
                        {formErrors.partnerId && (
                            <p className="text-sm text-red-500">{formErrors.partnerId}</p>
                        )}
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <Input
                            label={t('contacts.firstName')}
                            value={formData.firstName}
                            onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                            error={formErrors.firstName}
                            required
                        />
                        <Input
                            label={t('contacts.lastName')}
                            value={formData.lastName}
                            onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                            error={formErrors.lastName}
                            required
                        />
                    </div>
                    <Input
                        label={t('contacts.position')}
                        value={formData.position}
                        onChange={(e) => setFormData({ ...formData, position: e.target.value })}
                    />
                    <div className="grid grid-cols-2 gap-4">
                        <Input
                            label={t('contacts.email')}
                            type="email"
                            value={formData.email}
                            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                        />
                        <Input
                            label={t('contacts.phone')}
                            value={formData.phone}
                            onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                        />
                    </div>
                    <label className="flex items-center gap-2 cursor-pointer">
                        <input
                            type="checkbox"
                            checked={formData.isPrimary}
                            onChange={(e) => setFormData({ ...formData, isPrimary: e.target.checked })}
                            className="rounded border-[hsl(var(--border))]"
                        />
                        <span>{t('contacts.isPrimary')}</span>
                    </label>
                </div>
            </Modal>
        </div>
    );
}
