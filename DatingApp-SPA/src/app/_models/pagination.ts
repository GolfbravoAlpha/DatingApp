export interface Pagination {
    // replicate the information we're gettting from the paginationHeader.cs
    currentPage: number;
    itemsPerPage: number;
    totalItems: number;
    totalPages: number;
}

// we get back from the api the paginated users and the pagination infomormation such as
// {"currentPage":3,"itemsPerPage":3,"totalItems":17,"totalPages":6}

export class PaginatedResult<T> {
    result: T; //  store our users or messages in
    pagination: Pagination; // store pagination
}
