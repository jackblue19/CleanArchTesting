const AdminMoviePage = ({ navigate }) => {
    const { showToast } = useAuth();
    const [isEditing, setIsEditing] = useState(false);
    const [selectedMovie, setSelectedMovie] = useState(null);

    const handleEdit = (movie) => {
        setSelectedMovie(movie);
        setIsEditing(true);
    };

    const handleDelete = (movieId) => {
        showToast(`Đã xóa phim ID: ${movieId} (Mock API)`, 'danger');
        // Thực tế: Gọi API DELETE
    };

    return (
        <AdminLayout navigate={navigate} activePage="admin/movies">
            <div className="flex justify-between items-center mb-6 border-b pb-4">
                <h2 className="text-3xl font-bold text-gray-800">Quản Lý Phim</h2>
                <Button onClick={() => {setIsEditing(true); setSelectedMovie(null);}} variant="primary">Thêm Phim Mới</Button>
            </div>

            {isEditing && (
                <MovieForm
                    movie={selectedMovie}
                    onClose={() => { setIsEditing(false); setSelectedMovie(null); }}
                    showToast={showToast}
                />
            )}

            <div className="bg-white rounded-xl shadow-lg overflow-hidden mt-6">
                <div className="overflow-x-auto">
                    <table className="min-w-full divide-y divide-gray-200">
                        <thead className="bg-gray-50">
                            <tr>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">ID</th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Tiêu Đề</th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Thể Loại</th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Hành Động</th>
                            </tr>
                        </thead>
                        <tbody className="bg-white divide-y divide-gray-200">
                            {mockMovies.map(movie => (
                                <tr key={movie.id}>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{movie.id}</td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{movie.title}</td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{movie.genre}</td>
                                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium space-x-2">
                                        <Button variant="secondary" onClick={() => handleEdit(movie)}>Sửa</Button>
                                        <Button variant="danger" onClick={() => handleDelete(movie.id)}>Xóa</Button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>
        </AdminLayout>
    );
};

export default AdminMoviePage;
