import { useState, useRef, useEffect } from 'react';
import { ChevronDown, Check, Search } from 'lucide-react';
import { cn } from '@/lib/utils';

export interface ComboboxOption {
    label: string;
    value: string;
}

interface ComboboxProps {
    label?: string;
    options: ComboboxOption[];
    value?: string;
    onChange: (value: string) => void;
    placeholder?: string;
    searchPlaceholder?: string;
    className?: string;
    error?: string;
    disabled?: boolean;
}

export function Combobox({
    label,
    options,
    value,
    onChange,
    placeholder = "Select...",
    searchPlaceholder = "Search...",
    className,
    error,
    disabled = false
}: ComboboxProps) {
    const [isOpen, setIsOpen] = useState(false);
    const [searchTerm, setSearchTerm] = useState("");
    const containerRef = useRef<HTMLDivElement>(null);
    const inputRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
                setIsOpen(false);
            }
        };

        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    useEffect(() => {
        if (isOpen && inputRef.current) {
            inputRef.current.focus();
        }
        if (!isOpen) {
            setSearchTerm("");
        }
    }, [isOpen]);

    const handleSelect = (optionValue: string) => {
        onChange(optionValue);
        setIsOpen(false);
        setSearchTerm("");
    };

    const selectedOption = options.find(opt => opt.value === value);

    const filteredOptions = options.filter(option =>
        option.label.toLowerCase().includes(searchTerm.toLowerCase())
    );

    return (
        <div className={cn("space-y-1", className)} ref={containerRef}>
            {label && (
                <label className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70">
                    {label}
                </label>
            )}

            <div
                className={cn(
                    "flex h-10 w-full items-center justify-between rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 cursor-pointer",
                    error && "border-red-500 focus:ring-red-500",
                    isOpen && "ring-2 ring-ring ring-offset-2"
                )}
                onClick={() => !disabled && setIsOpen(!isOpen)}
            >
                <span className={cn("truncate", !selectedOption && "text-muted-foreground")}>
                    {selectedOption ? selectedOption.label : placeholder}
                </span>
                <ChevronDown className="h-4 w-4 opacity-50 shrink-0" />
            </div>

            {isOpen && (
                <div className="absolute z-[500] mt-1 w-[calc(100%-3rem)] max-w-md min-w-[200px] rounded-md border bg-popover text-popover-foreground shadow-md outline-none animate-in fade-in-0 zoom-in-95 bg-[hsl(var(--popover))]">
                    <div className="flex items-center border-b px-3" onClick={(e) => e.stopPropagation()}>
                        <Search className="mr-2 h-4 w-4 shrink-0 opacity-50" />
                        <input
                            ref={inputRef}
                            className="flex h-10 w-full rounded-md bg-transparent py-3 text-sm outline-none placeholder:text-muted-foreground disabled:cursor-not-allowed disabled:opacity-50"
                            placeholder={searchPlaceholder}
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                        />
                    </div>
                    <div className="max-h-60 overflow-y-auto p-1">
                        {filteredOptions.length === 0 ? (
                            <div className="py-6 text-center text-sm text-muted-foreground">
                                No options found.
                            </div>
                        ) : (
                            filteredOptions.map(option => (
                                <div
                                    key={option.value}
                                    className={cn(
                                        "relative flex w-full cursor-default select-none items-center rounded-sm py-1.5 pl-2 pr-8 text-sm outline-none hover:bg-accent hover:text-accent-foreground cursor-pointer transition-colors",
                                        value === option.value && "bg-accent text-accent-foreground"
                                    )}
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        handleSelect(option.value);
                                    }}
                                >
                                    <span className="truncate">{option.label}</span>
                                    {value === option.value && (
                                        <span className="absolute right-2 flex h-3.5 w-3.5 items-center justify-center">
                                            <Check className="h-4 w-4" />
                                        </span>
                                    )}
                                </div>
                            ))
                        )}
                    </div>
                </div>
            )}
            {error && <p className="text-sm font-medium text-red-500">{error}</p>}
        </div>
    );
}
