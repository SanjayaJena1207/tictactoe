import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { ApiError } from '../models/api-error.model';

export const errorInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const body = error.error as Partial<ApiError> | null;
      const apiError: ApiError = {
        status: error.status,
        title: body?.title ?? 'Request failed',
        detail: body?.detail ?? error.message,
        reason: body?.reason,
      };
      return throwError(() => apiError);
    }),
  );
