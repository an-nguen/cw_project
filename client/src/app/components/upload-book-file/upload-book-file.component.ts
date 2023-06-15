import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { UploadBookFileData } from './upload-book-file-data';
import { UploadBookFileResponse } from './upload-book-file-response';

@Component({
  selector: 'app-upload-book-file',
  templateUrl: './upload-book-file.component.html',
  styleUrls: ['./upload-book-file.component.scss'],
})
export class UploadBookFileComponent {
  public fileName: string = 'Выберите файл';
  public currentFile?: File;

  constructor(
    private _dialogRef: MatDialogRef<
      UploadBookFileComponent,
      UploadBookFileResponse
    >,
    @Inject(MAT_DIALOG_DATA) public data: UploadBookFileData
  ) {}

  public selectFile(event: Event) {
    if (
      event.target instanceof HTMLInputElement &&
      event.target.files &&
      event.target.files[0]
    ) {
      const file = event.target.files[0];
      this.currentFile = file;
      this.fileName = this.currentFile.name;
    } else {
      this.fileName = 'Select File';
    }
  }

  public onUploadClick() {
    if (this.currentFile) {
      this._dialogRef.close({
        file: this.currentFile,
      });
    }
  }

  public onCloseClick() {
    this._dialogRef.close(undefined);
  }
}
