import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, Shield, ShieldCheck } from 'lucide-react';
import { api } from '@/services/api';
import { DataTable, Button, Modal, Input, Badge, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';

interface Role {
    id: string;
    name: string;
    description: string | null;
    isSystemRole: boolean;
    permissionCount: number;
}

interface RoleFormData {
    name: string;
    description: string;
}

export function RolesPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [roles, setRoles] = useState<Role[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    // Modal state
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingRole, setEditingRole] = useState<Role | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Form state
    const [formData, setFormData] = useState<RoleFormData>({
        name: '',
        description: '',
    });
    const [formErrors, setFormErrors] = useState<Partial<RoleFormData>>({});

    const loadRoles = useCallback(async () => {
        setIsLoading(true);
        const result = await api.roles.getAll();
        if (result.success && result.data) {
            setRoles(result.data);
        }
        setIsLoading(false);
    }, []);

    useEffect(() => {
        loadRoles();
    }, [loadRoles]);

    const openCreateModal = () => {
        setEditingRole(null);
        setFormData({ name: '', description: '' });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const openEditModal = (role: Role) => {
        setEditingRole(role);
        setFormData({
            name: role.name,
            description: role.description || '',
        });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const validateForm = (): boolean => {
        const errors: Partial<RoleFormData> = {};

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
            const result = await api.roles.create(formData);
            if (result.success) {
                toast.success(editingRole ? t('roles.roleUpdated') : t('roles.roleCreated'));
                setIsModalOpen(false);
                loadRoles();
            } else {
                toast.error(t('common.error'), result.error);
            }
        } catch (err) {
            toast.error(t('common.error'), (err as Error).message);
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleDelete = async (role: Role) => {
        if (role.isSystemRole) {
            toast.error(t('common.error'), t('roles.cannotDeleteSystemRole'));
            return;
        }

        const confirmed = await dialog.danger({
            title: t('roles.deleteRole'),
            message: t('roles.confirmDeleteRole'),
            confirmText: t('common.delete'),
        });

        if (confirmed) {
            toast.info(t('common.info'), 'Delete not implemented yet');
        }
    };

    const columns: Column<Role>[] = [
        {
            key: 'name',
            header: t('roles.name'),
            render: (role) => (
                <div className="flex items-center gap-3">
                    <div
                        className={`
              w-10 h-10 rounded-lg flex items-center justify-center
              ${role.isSystemRole
                                ? 'bg-amber-100 dark:bg-amber-900/30 text-amber-600'
                                : 'bg-indigo-100 dark:bg-indigo-900/30 text-indigo-600'}
            `}
                    >
                        {role.isSystemRole ? (
                            <ShieldCheck className="w-5 h-5" />
                        ) : (
                            <Shield className="w-5 h-5" />
                        )}
                    </div>
                    <div>
                        <p className="font-medium">{role.name}</p>
                        {role.description && (
                            <p className="text-sm text-[hsl(var(--muted-foreground))]">
                                {role.description}
                            </p>
                        )}
                    </div>
                </div>
            ),
        },
        {
            key: 'isSystemRole',
            header: t('roles.isSystemRole'),
            render: (role) => (
                <Badge variant={role.isSystemRole ? 'warning' : 'default'}>
                    {role.isSystemRole ? t('common.yes') : t('common.no')}
                </Badge>
            ),
        },
        {
            key: 'permissionCount',
            header: t('roles.permissions'),
            render: (role) => (
                <span className="text-sm">{role.permissionCount} {t('roles.permissions').toLowerCase()}</span>
            ),
        },
    ];

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">{t('roles.title')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">
                        {t('roles.subtitle')}
                    </p>
                </div>
                <Button onClick={openCreateModal}>
                    <Plus className="w-4 h-4" />
                    {t('roles.createRole')}
                </Button>
            </div>

            {/* Table */}
            <DataTable
                data={roles}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                actions={(role) => (
                    <div className="flex items-center gap-1">
                        <button
                            onClick={() => openEditModal(role)}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                            title={t('common.edit')}
                        >
                            <Pencil className="w-4 h-4" />
                        </button>
                        {!role.isSystemRole && (
                            <button
                                onClick={() => handleDelete(role)}
                                className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600 transition-colors"
                                title={t('common.delete')}
                            >
                                <Trash2 className="w-4 h-4" />
                            </button>
                        )}
                    </div>
                )}
            />

            {/* Modal */}
            <Modal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title={editingRole ? t('roles.editRole') : t('roles.createRole')}
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
                        label={t('roles.name')}
                        value={formData.name}
                        onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                        error={formErrors.name}
                        required
                    />
                    <Input
                        label={t('roles.description')}
                        value={formData.description}
                        onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    />
                </div>
            </Modal>
        </div>
    );
}
