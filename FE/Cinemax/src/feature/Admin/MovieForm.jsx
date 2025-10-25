const MovieForm = ({ movie, onClose, showToast }) => {
    const [title, setTitle] = useState(movie?.title || '');
    const [genre, setGenre] = useState(movie?.genre || 'Hành Động');
    const isNew = !movie;

    const handleSubmit = (e) => {
        e.preventDefault();
        // Logic gọi API: POST nếu là mới, PUT nếu là sửa
        if (isNew) {
            showToast(`Đã thêm phim mới: ${title} (Mock API)`, 'success');
        } else {
            showToast(`Đã cập nhật phim ID: ${movie.id} thành ${title} (Mock API)`, 'success');
        }
        onClose();
    };

    return (
        <div className="p-6 bg-indigo-50 border border-indigo-200 rounded-xl shadow-inner mb-6">
            <h3 className="text-xl font-bold mb-4 text-indigo-800">{isNew ? 'Thêm Phim Mới' : `Sửa Phim: ${movie.title}`}</h3>
            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="block text-sm font-medium text-gray-700">Tên Phim</label>
                    <input
                        type="text"
                        value={title}
                        onChange={(e) => setTitle(e.target.value)}
                        className="w-full p-2 border border-gray-300 rounded-lg"
                        required
                    />
                </div>
                <div>
                    <label className="block text-sm font-medium text-gray-700">Thể Loại</label>
                    <select
                        value={genre}
                        onChange={(e) => setGenre(e.target.value)}
                        className="w-full p-2 border border-gray-300 rounded-lg"
                    >
                        <option>Hành Động</option>
                        <option>Lãng Mạn</option>
                        <option>Kinh Dị</option>
                    </select>
                </div>
                <div className="flex space-x-3 justify-end">
                    <Button type="button" variant="secondary" onClick={onClose}>Hủy</Button>
                    <Button type="submit" variant="primary">{isNew ? 'Lưu' : 'Cập Nhật'}</Button>
                </div>
            </form>
        </div>
    );
};
export default MovieForm;