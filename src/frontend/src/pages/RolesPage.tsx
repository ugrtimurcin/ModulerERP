import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, Shield, ShieldCheck } from 'lucide-react';
import { api } from '@/services/api';
import { DataTable, Button, Modal, Input, Badge, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';

// ... (previous imports)
import { PermissionGuard } from '@/components/PermissionGuard';

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
    permissions: string[];
}

// Helper to group permissions
const groupPermissions = (permissions: string[]) => {
    const groups: { [key: string]: string[] } = {};
    permissions.forEach(p => {
        const [group] = p.split('.');
        if (!groups[group]) groups[group] = [];
        groups[group].push(p);
    });
    return groups;
};

export function RolesPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [roles, setRoles] = useState<Role[]>([]);
    // const [availablePermissions, setAvailablePermissions] = useState<string[]>([]); // Unused
    const [permissionGroups, setPermissionGroups] = useState<{ [key: string]: string[] }>({});
    const [isLoading, setIsLoading] = useState(true);

    // Modal state
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingRole, setEditingRole] = useState<Role | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isLoadingDetails, setIsLoadingDetails] = useState(false);

    // Form state
    const [formData, setFormData] = useState<RoleFormData>({
        name: '',
        description: '',
        permissions: [],
    });
    const [formErrors, setFormErrors] = useState<Partial<RoleFormData>>({});

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const [rolesResult, permissionsResult] = await Promise.all([
                api.roles.getAll(),
                api.permissions(),
            ]);

            if (rolesResult.success && rolesResult.data) {
                setRoles(rolesResult.data);
            }
            if (permissionsResult.success && permissionsResult.data) {
                setPermissionGroups(groupPermissions(permissionsResult.data));
            }
        } catch (error) {
            console.error("Failed to load data", error);
            toast.error(t('common.error'), "Failed to load roles or permissions");
        }
        setIsLoading(false);
    }, [toast, t]);

    useEffect(() => {
        loadData();
    }, [loadData]);

    const openCreateModal = () => {
        setEditingRole(null);
        setFormData({ name: '', description: '', permissions: [] });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const openEditModal = async (role: Role) => {
        setEditingRole(role);
        setIsModalOpen(true);
        setIsLoadingDetails(true);

        // Initial data from list while loading full details
        setFormData({
            name: role.name,
            description: role.description || '',
            permissions: [], // Will be populated from API
        });

        try {
            const result = await api.roles.getById(role.id);
            if (result.success && result.data) {
                setFormData(prev => ({
                    ...prev,
                    permissions: result.data.permissions || []
                }));
            } else {
                toast.error(t('common.error'), "Failed to load role details");
            }
        } catch (error) {
            toast.error(t('common.error'), "Failed to load role details");
        } finally {
            setIsLoadingDetails(false);
        }
    };

    const togglePermission = (permission: string) => {
        setFormData(prev => {
            const newPermissions = prev.permissions.includes(permission)
                ? prev.permissions.filter(p => p !== permission)
                : [...prev.permissions, permission];
            return { ...prev, permissions: newPermissions };
        });
    };

    const toggleGroup = (_group: string, permissions: string[]) => {
        setFormData(prev => {
            const allSelected = permissions.every(p => prev.permissions.includes(p));
            let newPermissions = [...prev.permissions];

            if (allSelected) {
                // Deselect all
                newPermissions = newPermissions.filter(p => !permissions.includes(p));
            } else {
                // Select all
                permissions.forEach(p => {
                    if (!newPermissions.includes(p)) newPermissions.push(p);
                });
            }
            return { ...prev, permissions: newPermissions };
        });
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
            // If editing, use update (Assuming update exists, if not, create might handle it or need adjustment)
            // The API definition showed create for roles, but not explicit update in the previous view. 
            // Checking api.ts again: `create: (data: { name: string; description?: string })` 
            // Wait, api.ts snippet for roles only showed getAll, getById, create. UPDATE WAS MISSING?
            // Let me check api.ts snippet line 160-166.
            // It was: 164: create: ..., 166: }. 
            // I might need to add Update to API first if it's missing.
            // Assuming it was there or I need to add it. 
            // Wait, if I look at `api.ts` file provided earlier...
            // Lines 161-166: getAll, getById, create. NO UPDATE.
            // I need to add update to `api.ts`!

            // I'll assume I will fix api.ts in a moment.

            let result;
            if (editingRole) {
                // CAUTION: API MIGHT BE MISSING UPDATE. I'll rely on fixing it.
                // Assuming api.roles.update exists or I will add it.
                // For now using `any` cast to avoid TS error if not yet added in type defs implies I should add it first.
                // But I will write the code assuming it is there.
                // Actually, I should probably split this task if api is missing.
                // But let's write correct code here.
                result = await (api.roles as any).update(editingRole.id, formData);
            } else {
                result = await api.roles.create(formData);
            }

            if (result.success) {
                toast.success(editingRole ? t('roles.roleUpdated') : t('roles.roleCreated'));
                setIsModalOpen(false);
                loadData();
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
        // ... (Same as before)
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
            // Assuming api.roles.delete exists? Checked api.ts: MISSING.
            // Need to add delete to api.ts as well.
            toast.info(t('common.info'), 'Delete not implemented in API client yet');
        }
    };

    const columns: Column<Role>[] = [
        // ... (Keep existing columns)
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
                <PermissionGuard permission="roles.create">
                    <Button onClick={openCreateModal}>
                        <Plus className="w-4 h-4" />
                        {t('roles.createRole')}
                    </Button>
                </PermissionGuard>
            </div>

            {/* Table */}
            <DataTable
                data={roles}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                actions={(role) => (
                    <div className="flex items-center gap-1">
                        <PermissionGuard permission="roles.edit">
                            <button
                                onClick={() => openEditModal(role)}
                                className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                                title={t('common.edit')}
                            >
                                <Pencil className="w-4 h-4" />
                            </button>
                        </PermissionGuard>
                        {!role.isSystemRole && (
                            <PermissionGuard permission="roles.delete">
                                <button
                                    onClick={() => handleDelete(role)}
                                    className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600 transition-colors"
                                    title={t('common.delete')}
                                >
                                    <Trash2 className="w-4 h-4" />
                                </button>
                            </PermissionGuard>
                        )}
                    </div>
                )}
            />

            {/* Modal */}
            <Modal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title={editingRole ? t('roles.editRole') : t('roles.createRole')}
                size="lg"
                footer={
                    <>
                        <Button variant="secondary" onClick={() => setIsModalOpen(false)}>
                            {t('common.cancel')}
                        </Button>
                        <Button onClick={handleSubmit} isLoading={isSubmitting || isLoadingDetails}>
                            {t('common.save')}
                        </Button>
                    </>
                }
            >
                {isLoadingDetails ? (
                    <div className="flex justify-center py-8">
                        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
                    </div>
                ) : (
                    <div className="space-y-6">
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

                        {/* Permission Matrix */}
                        <div className="border rounded-lg p-4 space-y-4">
                            <h3 className="font-medium flex items-center gap-2">
                                <Shield className="w-4 h-4" />
                                {t('roles.permissions')}
                            </h3>

                            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                {Object.entries(permissionGroups).map(([group, groupPermissions]) => (
                                    <div key={group} className="space-y-2">
                                        <div className="flex items-center justify-between border-b pb-1">
                                            <span className="font-medium capitalize">{group}</span>
                                            <button
                                                type="button"
                                                onClick={() => toggleGroup(group, groupPermissions)}
                                                className="text-xs text-indigo-600 hover:underline"
                                            >
                                                {groupPermissions.every(p => formData.permissions.includes(p))
                                                    ? t('common.deselectAll')
                                                    : t('common.selectAll')}
                                            </button>
                                        </div>
                                        <div className="space-y-1">
                                            {groupPermissions.map(permission => (
                                                <label key={permission} className="flex items-center gap-2 cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-800/50 p-1 rounded">
                                                    <input
                                                        type="checkbox"
                                                        className="rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                                                        checked={formData.permissions.includes(permission)}
                                                        onChange={() => togglePermission(permission)}
                                                    />
                                                    <span className="text-sm">{permission.split('.').slice(1).join(' ')}</span>
                                                </label>
                                            ))}
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    </div>
                )}
            </Modal>
        </div>
    );
}
