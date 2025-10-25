import AdminSider from "../../components/sider/AdminSider";

const AdminLayout = ({ children, navigate, activePage }) => {
    const { isAdmin } = useAuth();
    if (!isAdmin) {
        return <div className="p-10 text-red-600 font-bold text-center">Bạn không có quyền truy cập trang quản trị.</div>;
    }
    return (
        <div className="flex min-h-screen bg-gray-100">
            <AdminSider navigate={navigate} activePage={activePage} />
            <div className="flex-1 flex flex-col">
                <header className="bg-white shadow p-4">
                    <h1 className="text-2xl font-bold text-gray-800">Quản Trị Hệ Thống</h1>
                </header>
                <main className="flex-1 p-8">
                    {children}
                </main>
            </div>
        </div>
    );
};
export default AdminLayout;
