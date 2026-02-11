import { useState, useRef, useEffect } from 'react';
import { ChevronDown, X, Check } from 'lucide-react';
import { cn } from '@/lib/utils';

export interface Option {
    label: string;
    value: string;
}

interface MultiSelectProps {
    label?: string;
    options: Option[];
    value: string[];
    onChange: (value: string[]) => void;
    placeholder?: string;
    className?: string;
}

export function MultiSelect({
    label,
    options,
    value,
    onChange,
    placeholder = "Select...",
    className
}: MultiSelectProps) {
    const [isOpen, setIsOpen] = useState(false);
    const containerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
                setIsOpen(false);
            }
        };

        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    const handleSelect = (optionValue: string) => {
        if (value.includes(optionValue)) {
            onChange(value.filter(v => v !== optionValue));
        } else {
            onChange([...value, optionValue]);
        }
    };

    const handleRemove = (e: React.MouseEvent, optionValue: string) => {
        e.stopPropagation();
        onChange(value.filter(v => v !== optionValue));
    };

    const selectedOptions = options.filter(opt => value.includes(opt.value));

    return (
        <div className={cn("space-y-2", className)} ref={containerRef}>
            {label && <label className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">{label}</label>}

            <div
                className={cn(
                    "flex min-h-[40px] w-full flex-wrap items-center justify-between rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 cursor-pointer",
                    isOpen && "ring-2 ring-ring ring-offset-2"
                )}
                onClick={() => setIsOpen(!isOpen)}
            >
                <div className="flex flex-wrap gap-1">
                    {selectedOptions.length > 0 ? (
                        selectedOptions.map(option => (
                            <span
                                key={option.value}
                                className="inline-flex items-center rounded-sm border border-transparent bg-secondary px-1.5 py-0.5 text-xs font-semibold text-secondary-foreground transition-colors hover:bg-secondary/80"
                            >
                                {option.label}
                                <X
                                    className="ml-1 h-3 w-3 cursor-pointer text-muted-foreground hover:text-foreground"
                                    onClick={(e) => handleRemove(e, option.value)}
                                />
                            </span>
                        ))
                    ) : (
                        <span className="text-muted-foreground">{placeholder}</span>
                    )}
                </div>
                <ChevronDown className="h-4 w-4 opacity-50" />
            </div>

            {isOpen && (
                <div className="relative z-50">
                    <div className="absolute top-0 w-full rounded-md border bg-popover text-popover-foreground shadow-md outline-none animate-in fade-in-0 zoom-in-95">
                        <div className="max-h-64 overflow-auto p-1">
                            {options.length === 0 ? (
                                <div className="py-2 text-center text-sm text-muted-foreground">No options found.</div>
                            ) : (
                                options.map(option => (
                                    <div
                                        key={option.value}
                                        className={cn(
                                            "relative flex w-full cursor-default select-none items-center rounded-sm py-1.5 pl-2 pr-8 text-sm outline-none focus:bg-accent focus:text-accent-foreground data-[disabled]:pointer-events-none data-[disabled]:opacity-50 hover:bg-accent hover:text-accent-foreground cursor-pointer",
                                            value.includes(option.value) && "bg-accent text-accent-foreground"
                                        )}
                                        onClick={() => handleSelect(option.value)}
                                    >
                                        <span className="flex-1 truncate">{option.label}</span>
                                        {value.includes(option.value) && (
                                            <span className="absolute right-2 flex h-3.5 w-3.5 items-center justify-center">
                                                <Check className="h-4 w-4" />
                                            </span>
                                        )}
                                    </div>
                                ))
                            )}
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}
