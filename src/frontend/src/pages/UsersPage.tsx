import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2 } from 'lucide-react';
import { api } from '@/services/api';
import { DataTable, Button, Modal, Input, Badge, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';

interface User {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    isActive: boolean;
    createdAt: string;
    lastLoginDate: string | null;
}

interface UserFormData {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
}

export function UsersPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [users, setUsers] = useState<User[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [total, setTotal] = useState(0);
    const pageSize = 20;

    // Modal state
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingUser, setEditingUser] = useState<User | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Form state
    const [formData, setFormData] = useState<UserFormData>({
        email: '',
        password: '',
        firstName: '',
        lastName: '',
    });
    const [formErrors, setFormErrors] = useState<Partial<UserFormData>>({});

    const loadUsers = useCallback(async () => {
        setIsLoading(true);
        const result = await api.users.getAll(page, pageSize);
        if (result.success && result.data) {
            setUsers(result.data.data);
            setTotal(result.data.totalCount);
        }
        setIsLoading(false);
    }, [page]);

    useEffect(() => {
        loadUsers();
    }, [loadUsers]);

    const openCreateModal = () => {
        setEditingUser(null);
        setFormData({ email: '', password: '', firstName: '', lastName: '' });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const openEditModal = (user: User) => {
        setEditingUser(user);
        setFormData({
            email: user.email,
            password: '',
            firstName: user.firstName,
            lastName: user.lastName,
        });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const validateForm = (): boolean => {
        const errors: Partial<UserFormData> = {};

        if (!formData.firstName.trim()) {
            errors.firstName = t('common.required');
        }
        if (!formData.lastName.trim()) {
            errors.lastName = t('common.required');
        }
        if (!formData.email.trim()) {
            errors.email = t('common.required');
        } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
            errors.email = t('common.invalidEmail');
        }
        if (!editingUser && !formData.password) {
            errors.password = t('common.required');
        }

        setFormErrors(errors);
        return Object.keys(errors).length === 0;
    };

    const handleSubmit = async () => {
        if (!validateForm()) return;

        setIsSubmitting(true);
        try {
            if (editingUser) {
                const result = await api.users.update(editingUser.id, {
                    firstName: formData.firstName,
                    lastName: formData.lastName,
                });
                if (result.success) {
                    toast.success(t('users.userUpdated'));
                    setIsModalOpen(false);
                    loadUsers();
                } else {
                    toast.error(t('common.error'), result.error);
                }
            } else {
                const result = await api.users.create(formData);
                if (result.success) {
                    toast.success(t('users.userCreated'));
                    setIsModalOpen(false);
                    loadUsers();
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

    const handleDelete = async (user: User) => {
        const confirmed = await dialog.danger({
            title: t('users.deleteUser'),
            message: t('users.confirmDeleteUser'),
            confirmText: t('common.delete'),
        });

        if (confirmed) {
            const result = await api.users.delete(user.id);
            if (result.success) {
                toast.success(t('users.userDeleted'));
                loadUsers();
            } else {
                toast.error(t('common.error'), result.error);
            }
        }
    };

    const columns: Column<User>[] = [
        {
            key: 'user',
            header: t('users.firstName'),
            render: (user) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-full bg-gradient-to-r from-indigo-500 to-purple-600 text-white flex items-center justify-center text-sm font-medium">
                        {user.firstName[0]}
                        {user.lastName[0]}
                    </div>
                    <div>
                        <p className="font-medium">{user.firstName} {user.lastName}</p>
                        <p className="text-sm text-[hsl(var(--muted-foreground))]">{user.email}</p>
                    </div>
                </div>
            ),
        },
        {
            key: 'isActive',
            header: t('common.status'),
            render: (user) => (
                <Badge variant={user.isActive ? 'success' : 'error'}>
                    {user.isActive ? t('common.active') : t('common.inactive')}
                </Badge>
            ),
        },
        {
            key: 'createdAt',
            header: t('users.createdAt'),
            render: (user) => new Date(user.createdAt).toLocaleDateString(),
        },
        {
            key: 'lastLoginDate',
            header: t('users.lastLogin'),
            render: (user) =>
                user.lastLoginDate
                    ? new Date(user.lastLoginDate).toLocaleString()
                    : '-',
        },
    ];

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">{t('users.title')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">
                        {t('users.subtitle')}
                    </p>
                </div>
                <Button onClick={openCreateModal}>
                    <Plus className="w-4 h-4" />
                    {t('users.createUser')}
                </Button>
            </div>

            {/* Table */}
            <DataTable
                data={users}
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
                actions={(user) => (
                    <div className="flex items-center gap-1">
                        <button
                            onClick={() => openEditModal(user)}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                            title={t('common.edit')}
                        >
                            <Pencil className="w-4 h-4" />
                        </button>
                        <button
                            onClick={() => handleDelete(user)}
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
                title={editingUser ? t('users.editUser') : t('users.createUser')}
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
                    <div className="grid grid-cols-2 gap-4">
                        <Input
                            label={t('users.firstName')}
                            value={formData.firstName}
                            onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                            error={formErrors.firstName}
                            required
                        />
                        <Input
                            label={t('users.lastName')}
                            value={formData.lastName}
                            onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                            error={formErrors.lastName}
                            required
                        />
                    </div>
                    <Input
                        label={t('users.email')}
                        type="email"
                        value={formData.email}
                        onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                        error={formErrors.email}
                        disabled={!!editingUser}
                        required
                    />
                    <Input
                        label={editingUser ? t('users.newPassword') : t('auth.password')}
                        type="password"
                        value={formData.password}
                        onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                        error={formErrors.password}
                        hint={editingUser ? t('users.leaveBlankToKeep') : undefined}
                        required={!editingUser}
                    />
                </div>
            </Modal>
        </div>
    );
}
