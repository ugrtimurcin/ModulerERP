import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, Building2, Users } from 'lucide-react';
import { DataTable, Button, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';
import { DepartmentDialog } from './DepartmentDialog';
import { api } from '@/lib/api';

interface Department {
    id: string;
    name: string;
    description: string | null;
    managerId: string | null;
    managerName: string | null;
    employeeCount?: number;
}

interface Employee {
    id: string;
    firstName: string;
    lastName: string;
}

export function DepartmentsPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [departments, setDepartments] = useState<Department[]>([]);
    const [employees, setEmployees] = useState<Employee[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [dialogOpen, setDialogOpen] = useState(false);
    const [editingDept, setEditingDept] = useState<Department | null>(null);

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const [deptData, empData] = await Promise.all([
                api.get<Department[]>('/hr/departments'),
                api.get<Employee[]>('/hr/employees')
            ]);

            setDepartments(deptData);
            setEmployees(empData);
        } catch {
            toast.error(t('common.error'));
        }
        setIsLoading(false);
    }, [toast]);

    useEffect(() => { loadData(); }, [loadData]);

    const handleCreate = () => { setEditingDept(null); setDialogOpen(true); };
    const handleEdit = (dept: Department) => { setEditingDept(dept); setDialogOpen(true); };

    const handleDelete = async (dept: Department) => {
        const confirmed = await dialog.danger({
            title: t('hr.deleteDepartment'),
            message: t('hr.confirmDeleteDepartment', { name: dept.name }),
            confirmText: t('common.delete'),
        });

        if (confirmed) {
            try {
                await api.delete(`/hr/departments/${dept.id}`);
                toast.success(t('hr.departmentDeleted'));
                loadData();
            } catch {
                toast.error(t('common.error'));
            }
        }
    };

    const handleDialogClose = (saved: boolean) => {
        setDialogOpen(false);
        if (saved) loadData();
    };

    const columns: Column<Department>[] = [
        {
            key: 'name',
            header: t('common.name'),
            render: (dept) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-emerald-500 to-teal-500 flex items-center justify-center text-white">
                        <Building2 className="w-5 h-5" />
                    </div>
                    <div>
                        <p className="font-semibold">{dept.name}</p>
                        {dept.description && (
                            <p className="text-xs text-[hsl(var(--muted-foreground))] truncate max-w-[200px]">
                                {dept.description}
                            </p>
                        )}
                    </div>
                </div>
            ),
        },
        {
            key: 'managerName',
            header: t('hr.manager'),
            render: (dept) => dept.managerName ? (
                <span className="text-sm">{dept.managerName}</span>
            ) : (
                <span className="text-sm text-[hsl(var(--muted-foreground))]">—</span>
            ),
        },
        {
            key: 'employeeCount',
            header: t('hr.employees'),
            render: (dept) => (
                <div className="flex items-center gap-1 text-sm">
                    <Users className="w-4 h-4 text-[hsl(var(--muted-foreground))]" />
                    <span>{dept.employeeCount || 0}</span>
                </div>
            ),
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <Building2 className="w-6 h-6" />
                        {t('hr.departments')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('hr.departmentsSubtitle')}</p>
                </div>
                <Button onClick={handleCreate}>
                    <Plus className="w-4 h-4" />
                    {t('hr.createDepartment')}
                </Button>
            </div>

            <DataTable
                data={departments}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                actions={(dept) => (
                    <div className="flex items-center gap-1">
                        <button onClick={() => handleEdit(dept)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]" title={t('common.edit')}>
                            <Pencil className="w-4 h-4" />
                        </button>
                        <button onClick={() => handleDelete(dept)} className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600" title={t('common.delete')}>
                            <Trash2 className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            <DepartmentDialog open={dialogOpen} onClose={handleDialogClose} department={editingDept} employees={employees} />
        </div>
    );
}
