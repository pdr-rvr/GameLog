import React, { useState, useRef, useEffect } from 'react';
import './Dropdown.css';

const Dropdown = ({ trigger, children, className }) => {
    const [dropdownOpen, setDropdownOpen] = useState(false);
    const dropdownRef = useRef(null);

    const toggleDropdown = () => {
        setDropdownOpen(prev => !prev);
    };

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
                setDropdownOpen(false);
            }
        };

        document.addEventListener('mousedown', handleClickOutside);
        return () => {
            document.removeEventListener('mousedown', handleClickOutside);
        };
    }, []);

    return (
        <div className={`dropdown-container ${className}`} ref={dropdownRef}>
            <div onClick={toggleDropdown} className="dropdown-trigger">
                {trigger}
            </div>

            <div className={`dropdown-menu ${dropdownOpen ? 'open' : ''}`}>
                {children}
            </div>
        </div>
    );
};

export default Dropdown