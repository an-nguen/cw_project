import { HttpClient, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Book } from 'src/app/models/book';
import { environment } from 'src/environments/environment.development';

@Injectable({
  providedIn: 'root',
})
export class BookService {
  private readonly BASE_URL = `${environment.api}/books`;

  constructor(private _http: HttpClient) {}

  public getBooks(): Observable<Array<Book>> {
    return this._http.get<Array<Book>>(this.BASE_URL);
  }

  public addBook(book: Book): Observable<Book> {
    return this._http.post<Book>(this.BASE_URL, book);
  }

  public updateBook(book: Book): Observable<Book> {
    return this._http.put<Book>(this.BASE_URL, book);
  }

  public removeBook(id: string): Observable<void> {
    return this._http.delete<void>(this.BASE_URL + '/' + id);
  }

  public uploadBook(id: string, file: File): Observable<unknown> {
    const formData = new FormData();
    formData.set('file', file);
    return this._http.post<void>(`${this.BASE_URL}/upload/${id}`, formData);
  }

  public downloadBook(id: string): Observable<HttpResponse<Blob>> {
    return this._http.get(`${this.BASE_URL}/download/${id}`, {
      observe: 'response',
      responseType: 'blob',
    });
  }
}
