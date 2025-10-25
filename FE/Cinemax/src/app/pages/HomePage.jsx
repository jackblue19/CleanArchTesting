const HomePage = ({ navigate }) => {
  // Sử dụng useFetch để mô phỏng data fetching và quản lý trạng thái tải
  const fetchMovies = React.useCallback(() => mockApi.getMovies(), []);
  const { data: movies, loading, error } = useFetch(fetchMovies, []);

  return (
    <MainLayout navigate={navigate}>
      <h2 className="text-3xl font-extrabold text-gray-900 mb-8 border-b pb-4">
        Phim Đang Chiếu Tuyển Chọn
      </h2>
      {loading && <Spinner />}
      {error && <p className="text-red-500 text-center">Lỗi khi tải dữ liệu phim.</p>}
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
        {movies.map(movie => (
          <MovieCard key={movie.id} movie={movie} navigate={navigate} />
        ))}
      </div>
    </MainLayout>
  );
};
export default HomePage;