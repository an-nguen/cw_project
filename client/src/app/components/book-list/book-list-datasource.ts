import { DataSource } from '@angular/cdk/collections';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { map, mergeMap } from 'rxjs/operators';
import { Observable, merge, ReplaySubject, of } from 'rxjs';
import { Book } from 'src/app/models/book';

export interface BookListItem extends Book {}

/**
 * Data source for the BookList view. This class should
 * encapsulate all logic for fetching and manipulating the displayed data
 * (including sorting, pagination, and filtering).
 */
export class BookListDataSource extends DataSource<BookListItem> {
  private _dataStream = new ReplaySubject<BookListItem[]>();
  public paginator: MatPaginator | undefined;
  public sort: MatSort | undefined;
  public dataLength = 0;

  constructor() {
    super();
    this._dataStream.subscribe((items) => {
      this.dataLength = items.length;
    });
  }

  /**
   * Connect this data source to the table. The table will only update when
   * the returned stream emits new items.
   * @returns A stream of the items to be rendered.
   */
  public connect(): Observable<BookListItem[]> {
    if (this.paginator && this.sort) {
      // Combine everything that affects the rendered data into one update
      // stream for the data-table to consume.
      return merge(
        this._dataStream,
        this.paginator.page,
        this.sort.sortChange
      ).pipe(
        mergeMap(() => {
          return this.getPagedDataStream(
            this.getSortedDataStream(this._dataStream)
          );
        })
      );
    } else {
      throw Error(
        'Please set the paginator and sort on the data source before connecting.'
      );
    }
  }

  /**
   *  Called when the table is being destroyed. Use this function, to clean up
   * any open connections or free any held resources that were set up during connect.
   */
  public disconnect(): void {}

  public setData(data: BookListItem[]): void {
    this._dataStream.next(data);
  }

  public getData() {
    return this._dataStream;
  }

  /**
   * Paginate the data (client-side). If you're using server-side pagination,
   * this would be replaced by requesting the appropriate data from the server.
   */
  public getPagedDataStream(data: Observable<BookListItem[]>) {
    if (this.paginator) {
      const startIndex = this.paginator.pageIndex * this.paginator.pageSize;
      return data.pipe(
        map((items) => items.slice(startIndex, this.paginator?.pageSize))
      );
    } else {
      return data;
    }
  }

  /**
   * Sort the data (client-side). If you're using server-side sorting,
   * this would be replaced by requesting the appropriate data from the server.
   */
  public getSortedDataStream(data: Observable<BookListItem[]>) {
    if (!this.sort || !this.sort.active || this.sort.direction === '') {
      return data;
    }

    return data.pipe(
      map((items) =>
        items.sort((a, b) => {
          const isAsc = this.sort?.direction === 'asc';
          switch (this.sort?.active) {
            case 'title':
              return compare(a.title, b.title, isAsc);
            case 'isbn':
              return compare(a.isbn, b.isbn, isAsc);
            default:
              return 0;
          }
        })
      )
    );
  }
}

/** Simple sort comparator for example ID/Name columns (for client-side sorting). */
function compare(
  a: string | number,
  b: string | number,
  isAsc: boolean
): number {
  return (a < b ? -1 : 1) * (isAsc ? 1 : -1);
}
