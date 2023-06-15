import { Author } from 'src/app/models/author';
import { Book } from 'src/app/models/book';

export interface BookEditDialogData {
  book?: Book;
  authors: Author[];
}
