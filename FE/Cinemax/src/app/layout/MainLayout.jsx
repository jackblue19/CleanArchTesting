import Footer from "../../components/footer/Footer";
import Header from "../../components/header/Header";

const MainLayout = ({ children, navigate }) => (
  <div className="flex flex-col min-h-screen bg-gray-50">
    <Header navigate={navigate} />
    <main className="flex-grow max-w-7xl mx-auto w-full px-4 sm:px-6 lg:px-8 py-8">
      {children}
    </main>
    <Footer />
  </div>
);
export default MainLayout;