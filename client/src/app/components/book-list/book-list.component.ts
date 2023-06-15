import {
  AfterViewInit,
  Component,
  EventEmitter,
  Input,
  Output,
  ViewChild,
} from '@angular/core';
import { MatTable } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { Book } from 'src/app/models/book';
import { BookListDataSource, BookListItem } from './book-list-datasource';

@Component({
  selector: 'app-book-list',
  templateUrl: './book-list.component.html',
  styleUrls: ['./book-list.component.scss'],
})
export class BookListComponent implements AfterViewInit {
  @ViewChild(MatPaginator) public paginator!: MatPaginator;
  @ViewChild(MatSort) public sort!: MatSort;
  @ViewChild(MatTable) public table!: MatTable<BookListItem>;
  @Input() public dataSource?: BookListDataSource;

  @Output()
  public edit = new EventEmitter<Book>();

  @Output()
  public delete = new EventEmitter<string>();

  @Output()
  public upload = new EventEmitter<string>();

  @Output()
  public download = new EventEmitter<string>();

  public onEditClick(book: Book): void {
    this.edit.emit(book);
  }

  public onDeleteClick(id: string): void {
    this.delete.emit(id);
  }

  public onUploadClick(id: string): void {
    this.upload.emit(id);
  }

  public onDownloadClick(id: string): void {
    this.download.emit(id);
  }

  /** Columns displayed in the table. Columns IDs can be added, removed, or reordered. */
  public displayedColumns = ['title', 'isbn', 'actions'];

  constructor() {}

  public ngAfterViewInit(): void {
    if (this.dataSource) {
      this.dataSource.sort = this.sort;
      this.dataSource.paginator = this.paginator;
      this.table.dataSource = this.dataSource;
    }
  }
}
