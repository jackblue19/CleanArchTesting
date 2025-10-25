const DetailPage = ({ navigate, movieId }) => {
  const fetchDetail = React.useCallback(() => mockApi.getMovieDetail(movieId), [movieId]);
  const fetchShowtimes = React.useCallback(() => mockApi.getShowtimes(movieId), [movieId]);

  const { data: movie, loading: loadingMovie, error: errorMovie } = useFetch(fetchDetail, null);
  const { data: showtimes, loading: loadingShowtimes, error: errorShowtimes } = useFetch(fetchShowtimes, []);

  if (loadingMovie) return <MainLayout navigate={navigate}><Spinner /></MainLayout>;
  if (errorMovie || !movie) return <MainLayout navigate={navigate}><div className="p-10 text-red-600 text-center">Không tìm thấy phim.</div></MainLayout>;

  return (
    <MainLayout navigate={navigate}>
      <div className="flex flex-col md:flex-row gap-8 bg-white p-6 rounded-xl shadow-lg">
        <img
          src={movie.poster}
          alt={movie.title}
          className="w-full md:w-64 h-96 object-cover rounded-lg shadow-xl flex-shrink-0"
          onError={(e) => { e.target.onerror = null; e.target.src = `https://placehold.co/300x450/4B5563/FFFFFF?text=${movie.title.replace(/\s/g, '+')}` }}
        />
        <div className="flex-grow">
          <h2 className="text-4xl font-extrabold text-gray-900 mb-3">{movie.title}</h2>
          <p className="text-indigo-600 font-semibold mb-4">{movie.genre} | {movie.duration} phút</p>
          <p className="text-gray-600 mb-6">Mô tả tóm tắt về phim {movie.title}. Đây là nơi hiển thị nội dung chi tiết được lấy từ API.</p>

          <h3 className="text-2xl font-bold text-gray-800 mb-4 border-t pt-4">Lịch Chiếu</h3>
          {loadingShowtimes && <Spinner />}
          {errorShowtimes && <p className="text-red-500">Lỗi khi tải lịch chiếu.</p>}
          <div className="space-y-3">
            {showtimes.length > 0 ? showtimes.map(st => (
              <div key={st.id} className="flex flex-col sm:flex-row justify-between items-start sm:items-center bg-gray-50 p-3 rounded-lg border">
                <div className="flex space-x-4 mb-2 sm:mb-0">
                    <span className="font-semibold text-gray-700">{st.date}</span>
                    <span className="text-indigo-600 font-bold text-lg">{st.time}</span>
                    <span className="text-gray-500">({st.theater})</span>
                </div>
                <Button onClick={() => navigate(`booking/${st.id}`)} variant="primary" className="text-sm w-full sm:w-auto">
                  Đặt Vé ({st.price.toLocaleString()} VNĐ)
                </Button>
              </div>
            )) : <p className="text-gray-500">Hiện không có lịch chiếu nào.</p>}
          </div>
        </div>
      </div>
    </MainLayout>
  );
};
export default DetailPage;