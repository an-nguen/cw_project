import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Book } from 'src/app/models/book';

@Component({
  selector: 'app-book-list',
  templateUrl: './book-list.component.html',
  styleUrls: ['./book-list.component.scss'],
})
export class BookListComponent {
  @Input()
  public books: Array<Book> = [];

  @Output()
  public edit = new EventEmitter<Book>();

  @Output()
  public delete = new EventEmitter<string>();
}
