import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import {
    Plus, Edit, Trash, ChevronRight, ChevronDown
} from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { Modal } from '@/components/ui/Modal';
import { useDialog } from '@/components/ui/Dialog';
import { projectService } from '@/services/projectService';
import type { ProjectTaskDto, CreateProjectTaskDto, UpdateProjectTaskDto } from '@/types/project';
import { ProjectTaskStatus } from '@/types/project';

interface TasksTabProps {
    projectId: string;
}

export function TasksTab({ projectId }: TasksTabProps) {
    const { t } = useTranslation();
    const { confirm } = useDialog();
    const [loading, setLoading] = useState(true);
    const [tasks, setTasks] = useState<ProjectTaskDto[]>([]);
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingTask, setEditingTask] = useState<ProjectTaskDto | null>(null);

    // Form State
    const [formData, setFormData] = useState<Partial<CreateProjectTaskDto>>({
        name: '',
        startDate: '',
        dueDate: '',
        assignedEmployeeId: undefined
    });

    useEffect(() => {
        loadTasks();
    }, [projectId]);

    const loadTasks = async () => {
        try {
            const response = await projectService.tasks.getByProject(projectId);
            if (response.success && response.data) {
                setTasks(buildTree(response.data));
            }
        } catch (error) {
            console.error('Failed to load tasks', error);
        } finally {
            setLoading(false);
        }
    };

    // Transform flat list to tree
    const buildTree = (flatTasks: ProjectTaskDto[]): ProjectTaskDto[] => {
        const taskMap: { [key: string]: ProjectTaskDto } = {};
        const tree: ProjectTaskDto[] = [];

        // First pass: create map and initialize children
        flatTasks.forEach(task => {
            taskMap[task.id] = { ...task, children: [] };
        });

        // Second pass: link children to parents
        flatTasks.forEach(task => {
            if (task.parentTaskId && taskMap[task.parentTaskId]) {
                taskMap[task.parentTaskId].children?.push(taskMap[task.id]);
            } else {
                tree.push(taskMap[task.id]);
            }
        });

        return tree;
    };

    const handleAddClick = (parent: ProjectTaskDto | null = null) => {
        setEditingTask(null);
        setFormData({
            projectId, // Ensure projectId is set
            name: '',
            startDate: new Date().toISOString().split('T')[0],
            dueDate: new Date().toISOString().split('T')[0],
            parentTaskId: parent?.id
        });
        setIsModalOpen(true);
    };

    const handleEditClick = (task: ProjectTaskDto) => {
        setEditingTask(task);
        setFormData({
            projectId,
            name: task.name,
            startDate: task.startDate.split('T')[0],
            dueDate: task.dueDate.split('T')[0],
            parentTaskId: task.parentTaskId,
            assignedEmployeeId: task.assignedEmployeeId
        });
        setIsModalOpen(true);
    };

    const handleDeleteClick = (task: ProjectTaskDto) => {
        confirm({
            title: t('projects.tasks.deleteTask'),
            message: t('common.thisActionCannotBeUndone'),
            confirmText: t('common.delete'),
            onConfirm: async () => {
                try {
                    await projectService.tasks.delete(task.id);
                    loadTasks();
                } catch (error) {
                    console.error('Failed to delete task', error);
                }
            }
        });
    };

    const handleSubmit = async () => {
        try {
            if (!formData.name || !formData.startDate || !formData.dueDate) return;

            if (editingTask) {
                await projectService.tasks.update(editingTask.id, {
                    id: editingTask.id,
                    projectId,
                    name: formData.name,
                    startDate: formData.startDate,
                    dueDate: formData.dueDate,
                    parentTaskId: formData.parentTaskId,
                    assignedEmployeeId: formData.assignedEmployeeId
                } as UpdateProjectTaskDto);
            } else {
                await projectService.tasks.create({
                    projectId,
                    name: formData.name!,
                    startDate: formData.startDate,
                    dueDate: formData.dueDate,
                    parentTaskId: formData.parentTaskId,
                    assignedEmployeeId: formData.assignedEmployeeId
                } as CreateProjectTaskDto);
            }

            setIsModalOpen(false);
            loadTasks();
        } catch (error) {
            console.error('Failed to save task', error);
        }
    };

    if (loading) return <div>{t('common.loading')}</div>;

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center">
                <h3 className="text-lg font-medium">{t('projects.tabs.tasks')}</h3>
                <Button onClick={() => handleAddClick(null)}>
                    <Plus className="mr-2 h-4 w-4" />
                    {t('projects.tasks.addTask')}
                </Button>
            </div>

            <div className="rounded-md border bg-card">
                <div className="p-4">
                    {tasks.length === 0 ? (
                        <div className="text-center text-muted-foreground py-8">
                            {t('common.noData')}
                        </div>
                    ) : (
                        <div className="space-y-2">
                            {tasks.map(task => (
                                <TaskItem
                                    key={task.id}
                                    task={task}
                                    level={0}
                                    onAddSub={handleAddClick}
                                    onEdit={handleEditClick}
                                    onDelete={handleDeleteClick}
                                />
                            ))}
                        </div>
                    )}
                </div>
            </div>

            <Modal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title={editingTask ? t('projects.tasks.editTask') : t('projects.tasks.addTask')}
            >
                <div className="space-y-4 py-4">
                    <Input
                        label={t('projects.tasks.taskName')}
                        value={formData.name}
                        onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    />
                    <div className="grid grid-cols-2 gap-4">
                        <Input
                            label={t('projects.startDate')}
                            type="date"
                            value={formData.startDate}
                            onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
                        />
                        <Input
                            label={t('projects.tasks.dueDate')}
                            type="date"
                            value={formData.dueDate}
                            onChange={(e) => setFormData({ ...formData, dueDate: e.target.value })}
                        />
                    </div>
                    {/* Employee Select would go here */}

                    <div className="flex justify-end space-x-2 mt-4">
                        <Button variant="ghost" onClick={() => setIsModalOpen(false)}>{t('common.cancel')}</Button>
                        <Button onClick={handleSubmit}>{t('common.save')}</Button>
                    </div>
                </div>
            </Modal>
        </div>
    );
}

interface TaskItemProps {
    task: ProjectTaskDto;
    level: number;
    onAddSub: (t: ProjectTaskDto) => void;
    onEdit: (t: ProjectTaskDto) => void;
    onDelete: (t: ProjectTaskDto) => void;
}

function TaskItem({ task, level, onAddSub, onEdit, onDelete }: TaskItemProps) {
    const [expanded, setExpanded] = useState(true);
    const hasChildren = task.children && task.children.length > 0;

    return (
        <div className="group">
            <div
                className="flex items-center p-2 hover:bg-muted/50 rounded-md border-b border-transparent hover:border-border transition-colors"
                style={{ paddingLeft: `${level * 20 + 8}px` }}
            >
                <button
                    onClick={() => setExpanded(!expanded)}
                    className={`mr-2 p-0.5 rounded-sm hover:bg-muted ${hasChildren ? 'visible' : 'invisible'}`}
                >
                    {expanded ? <ChevronDown className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />}
                </button>

                <div className="flex-1 flex items-center gap-3">
                    <span className="font-medium">{task.name}</span>

                    <div className="flex items-center gap-2 text-xs text-muted-foreground ml-auto">
                        <span className={`px-2 py-0.5 rounded-full ${task.status === ProjectTaskStatus.Completed ? 'bg-green-100 text-green-800' :
                            task.status === ProjectTaskStatus.InProgress ? 'bg-blue-100 text-blue-800' :
                                'bg-gray-100 text-gray-800'
                            }`}>
                            {task.completionPercentage}%
                        </span>

                        <div className="opacity-0 group-hover:opacity-100 flex items-center gap-1 transition-opacity">
                            <Button variant="ghost" size="icon" className="h-6 w-6" onClick={() => onAddSub(task)}>
                                <Plus className="h-3 w-3" />
                            </Button>
                            <Button variant="ghost" size="icon" className="h-6 w-6" onClick={() => onEdit(task)}>
                                <Edit className="h-3 w-3" />
                            </Button>
                            <Button variant="ghost" size="icon" className="h-6 w-6 text-red-500 hover:text-red-600" onClick={() => onDelete(task)}>
                                <Trash className="h-3 w-3" />
                            </Button>
                        </div>
                    </div>
                </div>
            </div>

            {expanded && hasChildren && (
                <div className="border-l border-dashed ml-4 border-muted-foreground/20">
                    {task.children!.map(child => (
                        <TaskItem
                            key={child.id}
                            task={child}
                            level={level + 1}
                            onAddSub={onAddSub}
                            onEdit={onEdit}
                            onDelete={onDelete}
                        />
                    ))}
                </div>
            )}
        </div>
    );
}
