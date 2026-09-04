import { Injectable } from '@angular/core';
import { OfflineInspectionDraft } from '../models/offline-inspection-draft.models';

interface StoredOfflineInspectionDraft extends OfflineInspectionDraft {
  readonly key: string;
}

@Injectable({ providedIn: 'root' })
export class OfflineInspectionDraftStore {
  private readonly databaseName = 'siteinspect-offline';
  private readonly storeName = 'inspection-drafts';
  private databasePromise: Promise<IDBDatabase> | null = null;

  async get(userId: string, inspectionId: string): Promise<OfflineInspectionDraft | null> {
    const database = await this.openDatabase();

    return new Promise((resolve, reject) => {
      const transaction = database.transaction(this.storeName, 'readonly');
      const request = transaction.objectStore(this.storeName).get(this.key(userId, inspectionId));

      request.onsuccess = () => {
        const stored = request.result as StoredOfflineInspectionDraft | undefined;
        if (!stored) {
          resolve(null);
          return;
        }

        resolve({
          userId: stored.userId,
          inspectionId: stored.inspectionId,
          rowVersion: stored.rowVersion,
          observations: stored.observations,
          savedAtUtc: stored.savedAtUtc,
        });
      };
      request.onerror = () => reject(request.error ?? new Error('Unable to read the local draft.'));
    });
  }

  async save(draft: OfflineInspectionDraft): Promise<void> {
    const database = await this.openDatabase();

    return new Promise((resolve, reject) => {
      const transaction = database.transaction(this.storeName, 'readwrite');
      transaction.objectStore(this.storeName).put({
        ...draft,
        key: this.key(draft.userId, draft.inspectionId),
      } satisfies StoredOfflineInspectionDraft);

      transaction.oncomplete = () => resolve();
      transaction.onerror = () => reject(transaction.error ?? new Error('Unable to save the local draft.'));
      transaction.onabort = () => reject(transaction.error ?? new Error('Saving the local draft was cancelled.'));
    });
  }

  async delete(userId: string, inspectionId: string): Promise<void> {
    const database = await this.openDatabase();

    return new Promise((resolve, reject) => {
      const transaction = database.transaction(this.storeName, 'readwrite');
      transaction.objectStore(this.storeName).delete(this.key(userId, inspectionId));

      transaction.oncomplete = () => resolve();
      transaction.onerror = () => reject(transaction.error ?? new Error('Unable to delete the local draft.'));
      transaction.onabort = () => reject(transaction.error ?? new Error('Deleting the local draft was cancelled.'));
    });
  }

  private openDatabase(): Promise<IDBDatabase> {
    if (!globalThis.indexedDB) {
      return Promise.reject(new Error('Offline storage is not supported by this browser.'));
    }

    this.databasePromise ??= new Promise((resolve, reject) => {
      const request = globalThis.indexedDB.open(this.databaseName, 1);

      request.onupgradeneeded = () => {
        if (!request.result.objectStoreNames.contains(this.storeName)) {
          request.result.createObjectStore(this.storeName, { keyPath: 'key' });
        }
      };
      request.onsuccess = () => {
        request.result.onversionchange = () => request.result.close();
        resolve(request.result);
      };
      request.onerror = () => reject(request.error ?? new Error('Unable to open offline storage.'));
      request.onblocked = () => reject(new Error('Offline storage is blocked by another tab.'));
    });

    return this.databasePromise;
  }

  private key(userId: string, inspectionId: string): string {
    return `${userId}:${inspectionId}`;
  }
}
