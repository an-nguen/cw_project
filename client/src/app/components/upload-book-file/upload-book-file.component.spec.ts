import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UploadBookFileComponent } from './upload-book-file.component';

describe('UploadBookFileComponent', () => {
  let component: UploadBookFileComponent;
  let fixture: ComponentFixture<UploadBookFileComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [UploadBookFileComponent]
    });
    fixture = TestBed.createComponent(UploadBookFileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
