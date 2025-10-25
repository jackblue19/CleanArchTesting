const AdminDashboard = ({ navigate }) => (
    <AdminLayout navigate={navigate} activePage="admin">
        <h2 className="text-3xl font-bold mb-6 text-gray-800">Bảng Điều Khiển Quản Trị</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <StatsCard title="Tổng Doanh Thu" value="1.25 Tỷ VNĐ" color="bg-green-100 text-green-800" />
            <StatsCard title="Số Lượt Đặt" value="15,482" color="bg-blue-100 text-blue-800" />
            <StatsCard title="Phim Đang Chiếu" value={mockMovies.length} color="bg-purple-100 text-purple-800" />
        </div>
        <div className="mt-8 p-6 bg-white rounded-xl shadow-lg">
            <h3 className="text-2xl font-semibold mb-4 border-b pb-2">Hành Động Nhanh</h3>
            <div className="flex space-x-4">
                <Button onClick={() => navigate('admin/movies')} variant="primary">Quản Lý Phim</Button>
                <Button onClick={() => navigate('admin/showtimes')} variant="secondary">Quản Lý Lịch Chiếu</Button>
            </div>
        </div>
    </AdminLayout>
);

export default AdminDashboard;
const StatsCard = ({ title, value, color }) => (
    <div className={`p-6 rounded-xl shadow-lg ${color}`}>
        <p className="text-sm font-medium opacity-80">{title}</p>
        <p className="text-3xl font-extrabold mt-1">{value}</p>
    </div>
);