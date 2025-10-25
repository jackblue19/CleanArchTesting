import React from "react";
import { useState } from "react";

const useFetch = (fetcher, initialData) => {
    const [data, setData] = useState(initialData);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    // Dùng useCallback để đảm bảo fetcher không thay đổi giữa các renders trừ khi dependencies thay đổi
    // Tuy nhiên, ở đây ta chỉ cần chạy 1 lần khi mount hoặc khi fetcher thay đổi
    React.useEffect(() => {
        let isMounted = true;
        setLoading(true);
        setError(null);

        fetcher()
            .then(res => {
                if (isMounted) setData(res);
            })
            .catch(err => {
                if (isMounted) setError(err);
            })
            .finally(() => {
                if (isMounted) setLoading(false);
            });

        return () => { isMounted = false; };
    }, [fetcher]);

    return { data, loading, error };
};
export default useFetch;
