
import React, { createContext, useContext, useState } from 'react';
import { cn } from '@/lib/utils';

interface TabsContextType {
    value: string;
    onValueChange: (value: string) => void;
}

const TabsContext = createContext<TabsContextType | undefined>(undefined);

interface TabsProps {
    defaultValue: string;
    value?: string;
    onValueChange?: (value: string) => void;
    children: React.ReactNode;
    className?: string;
}

export function Tabs({ defaultValue, value, onValueChange, children, className }: TabsProps) {
    const [localValue, setLocalValue] = useState(defaultValue);
    const currentValue = value !== undefined ? value : localValue;

    const handleValueChange = (newValue: string) => {
        setLocalValue(newValue);
        onValueChange?.(newValue);
    };

    return (
        <TabsContext.Provider value={{ value: currentValue, onValueChange: handleValueChange }}>
            <div className={cn('w-full', className)}>
                {children}
            </div>
        </TabsContext.Provider>
    );
}

export function TabsList({ children, className }: { children: React.ReactNode; className?: string }) {
    return (
        <div className={cn(
            'inline-flex h-10 items-center justify-center rounded-md bg-[hsl(var(--muted))] p-1 text-[hsl(var(--muted-foreground))]',
            className
        )}>
            {children}
        </div>
    );
}

interface TabsTriggerProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
    value: string;
}

export function TabsTrigger({ value, className, children, ...props }: TabsTriggerProps) {
    const context = useContext(TabsContext);
    if (!context) throw new Error('TabsTrigger must be used within Tabs');

    const isActive = context.value === value;

    return (
        <button
            type="button"
            onClick={() => context.onValueChange(value)}
            className={cn(
                'inline-flex items-center justify-center whitespace-nowrap rounded-sm px-3 py-1.5 text-sm font-medium ring-offset-background transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50',
                isActive
                    ? 'bg-[hsl(var(--background))] text-[hsl(var(--foreground))] shadow-sm'
                    : 'hover:bg-[hsl(var(--background))/50] hover:text-[hsl(var(--foreground))]',
                className
            )}
            {...props}
        >
            {children}
        </button>
    );
}

export function TabsContent({ value, children, className }: { value: string; children: React.ReactNode; className?: string }) {
    const context = useContext(TabsContext);
    if (!context) throw new Error('TabsContent must be used within Tabs');

    if (context.value !== value) return null;

    return (
        <div
            className={cn(
                'mt-2 ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2',
                className
            )}
        >
            {children}
        </div>
    );
}
