const AdminSider = ({ navigate, activePage }) => {
    const navItems = [
        { key: 'dashboard', label: 'Dashboard', path: 'admin' },
        { key: 'movies', label: 'Quản Lý Phim', path: 'admin/movies' },
        { key: 'showtimes', label: 'Quản Lý Lịch Chiếu', path: 'admin/showtimes' },
    ];

    return (
        <div className="w-56 bg-gray-900 text-white min-h-screen p-4 shadow-xl flex-shrink-0">
            <h2 className="text-xl font-bold mb-6 text-indigo-400 border-b border-gray-700 pb-3">Admin Panel</h2>
            <nav className="space-y-2">
                {navItems.map(item => (
                    <div
                        key={item.key}
                        onClick={() => navigate(item.path)}
                        className={`p-2 rounded-lg cursor-pointer transition duration-150 ${activePage.startsWith(item.path) ? 'bg-indigo-600 text-white' : 'hover:bg-gray-700 text-gray-300'}`}
                    >
                        {item.label}
                    </div>
                ))}
            </nav>
        </div>
    );
};

export default AdminSider;