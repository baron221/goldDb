<template>
<div class="settlement-history-container app-container">

    <template v-if="!isPayableSide">
      <settlement-history-filter
        :query="receivableQuery"
        :is-mobile="isMobile"
        company-category="RTL"
        company-label="소매점"
        @update:query="Object.assign(receivableQuery, $event)"
        @filter="handleReceivableFilter"
        @reset="resetReceivableQuery"
        @print-combined="printCombinedReceivableStatement"
      />

      <table class="order-history-summary-table">
        <thead>
          <tr>
            <th></th>
            <th>순금(g)</th>
            <th>공임 및 현금</th>
            <th>금액합계</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td class="summary-label">총 판매</td>
            <td>{{ (receivableSummary.totalChargeWeight || 0).toFixed(2) }}</td>
            <td>₩ {{ formatPrice(receivableSummary.totalChargeAmount) }}</td>
            <td>0</td>
          </tr>
          <tr>
            <td class="summary-label">총 결제</td>
            <td>{{ (receivableSummary.totalPaidWeight || 0).toFixed(2) }}</td>
            <td>₩ {{ formatPrice(receivableSummary.totalPaidAmount) }}</td>
            <td>0</td>
          </tr>
          <tr>
            <td class="summary-label">미수금</td>
            <td>{{ receivableOutstandingWeight.toFixed(2) }}</td>
            <td>₩ {{ formatPrice(receivableOutstandingAmount) }}</td>
            <td>0</td>
          </tr>
        </tbody>
      </table>

      <el-card shadow="never" style="margin-top: 1.25rem;">
        <base-table
          v-loading="receivableListLoading"
          :data="receivableList"
          :total="receivableTotal"
          v-model:page="receivableQuery.page"
          v-model:page-size="receivableQuery.pageSize"
          border
          row-key="id"
          style="width: 100%;"
          @change="fetchReceivableList"
        >
          <el-table-column label="거래번호" width="100" align="center" prop="id" />
          <el-table-column label="거래처" width="180" align="center">
            <template #default="{row}">
              {{ row.companyName || row.userDisplayName }}
            </template>
          </el-table-column>
          <el-table-column label="거래일자" width="160" align="center">
            <template #default="{row}">
              <span class="order-link" @click="openReceivableLedgerDetail(row)">{{ formatDate(row.createdAt) }}</span>
            </template>
          </el-table-column>
          <el-table-column label="거래 주문 수량" width="130" align="center">
            <template #default="{row}">
              {{ row.orderCount }}건
            </template>
          </el-table-column>
          <el-table-column label="정산 금액" width="150" align="right">
            <template #default="{row}">
              <span style="font-weight: bold; color: #67c23a;">₩ {{ formatPrice(row.amount) }}</span>
              <span v-if="row.weight > 0" style="color: #909399; font-size: 0.8125rem;"> ({{ row.weight.toFixed(2) }}g)</span>
            </template>
          </el-table-column>
          <el-table-column label="상태" width="100" align="center">
            <template #default="{row}">
              <el-tag v-if="row.isCancelled" type="info" size="small">취소됨</el-tag>
              <el-tag v-else-if="(row.outstandingChargeAmount || 0) > 0 || (row.outstandingChargeWeight || 0) > 0" type="warning" size="small">부분 정산</el-tag>
              <el-tag v-else type="success" size="small">완료</el-tag>
            </template>
          </el-table-column>
          <el-table-column label="미수 잔액" width="150" align="right">
            <template #default="{row}">
              <span v-if="!row.isCancelled && ((row.outstandingChargeAmount || 0) > 0 || (row.outstandingChargeWeight || 0) > 0)" style="color: #f56c6c; font-weight: bold;">
                ₩ {{ formatPrice(row.outstandingChargeAmount) }}
                <span v-if="row.outstandingChargeWeight > 0" style="color: #909399; font-size: 0.8125rem;"> ({{ row.outstandingChargeWeight.toFixed(2) }}g)</span>
              </span>
              <span v-else style="color: #c0c4cc;">-</span>
            </template>
          </el-table-column>
          <el-table-column label="메모" min-width="140" prop="memo" />
          <el-table-column label="작업" width="140" align="center" :fixed="!isMobile ? 'right' : false">
            <template #default="{row}">
              <el-button size="small" @click="handleIssueReceivableStatement(row)">거래명세서</el-button>
            </template>
          </el-table-column>
        </base-table>
      </el-card>

      <base-popup v-model="receivableLedgerDetailVisible" title="정산 상세" width="900px">
        <div v-if="receivableLedgerDetailRow" class="order-detail-expand" v-loading="receivableLedgerLoading">
          <table class="ledger-table" style="margin-bottom: 1.25rem;">
            <thead>
              <tr>
                <th>거래번호</th>
                <th>발행처</th>
                <th>거래처</th>
                <th>거래일자</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td class="ledger-readonly">{{ receivableLedgerDetailRow.id }}</td>
                <td class="ledger-readonly">{{ userStore.companyName || '-' }}</td>
                <td class="ledger-readonly">{{ receivableLedgerDetailRow.companyName || receivableLedgerDetailRow.userDisplayName }}</td>
                <td class="ledger-readonly">{{ formatDate(receivableLedgerDetailRow.createdAt) }}</td>
              </tr>
            </tbody>
          </table>
          <el-alert v-if="receivableLedgerDetailRow.isCancelled" type="info" :closable="false" show-icon>
            이 정산은 취소되어 적용된 제품 내역이 없습니다. 취소 시 연결된 청구 금액이 원래대로 복구되었습니다.
          </el-alert>
          <template v-else>
          <h4>적용 제품 내역</h4>
          <base-table :data="receivableLedgerDetailRow.appliedCharges || []" border size="small" style="width: 100%" row-key="chargeId">
            <el-table-column label="주문번호" width="180" align="center" prop="orderNo" />
            <el-table-column label="제품정보" min-width="280">
              <template #default="{row: item}">
                <div v-if="item.items && item.items.length > 0" class="product-info-list">
                  <div v-for="(p, idx) in item.items.slice(0, 3)" :key="idx" class="product-info-cell">
                    <el-image :src="p.photoUrl || defaultImage" fit="cover" class="product-thumb" style="width: 36px; height: 36px;" />
                    <div class="product-text">
                      <div class="product-name">
                        {{ p.productName || '-' }}
                        <span v-if="p.productNo" class="product-no-code">{{ p.productNo }}</span>
                      </div>
                      <div class="product-spec">
                        함량: {{ p.purity || '-' }} / 수량: {{ p.quantity }}개
                        <template v-if="p.color && p.color !== 'EMPTY'"> / 색상: {{ codeMap[p.color] || p.color }}</template>
                        <template v-if="p.size && p.size !== 'EMPTY'"> / 사이즈: {{ p.size }}</template>
                      </div>
                      <div v-if="p.memo" class="product-memo">메모: {{ p.memo }}</div>
                    </div>
                  </div>
                  <div v-if="item.items.length > 3" class="product-more">+{{ item.items.length - 3 }}건 더</div>
                </div>
                <span v-else>-</span>
              </template>
            </el-table-column>
            <el-table-column label="청구액" width="140" align="right">
              <template #default="{row: item}">
                <span style="font-weight: bold;">₩ {{ formatPrice(item.chargeAmount) }}</span>
              </template>
            </el-table-column>
            <el-table-column label="청구중량" width="120" align="right">
              <template #default="{row: item}">
                {{ (item.chargeWeight || 0).toFixed(2) }}g
              </template>
            </el-table-column>
            <el-table-column label="작업" width="120" align="center">
              <template #default="{row: item}">
                <el-button size="small" @click="handleIssueReceivableItemStatement(item, receivableLedgerDetailRow)">거래명세서</el-button>
              </template>
            </el-table-column>
          </base-table>

          <table v-if="receivableLedgerDetailRow.appliedCharges && receivableLedgerDetailRow.appliedCharges.length > 0" class="purity-summary-table expand-ledger">
            <thead>
              <tr>
                <th>14K 합계(g)</th>
                <th>18K 합계(g)</th>
                <th>순금 합계(g)</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>{{ getPurityBreakdownForReceivableRow(receivableLedgerDetailRow).p14.toFixed(2) }}</td>
                <td>{{ getPurityBreakdownForReceivableRow(receivableLedgerDetailRow).p18.toFixed(2) }}</td>
                <td>{{ getPurityBreakdownForReceivableRow(receivableLedgerDetailRow).pure.toFixed(2) }}</td>
              </tr>
            </tbody>
          </table>

          <table class="ledger-table expand-ledger">
            <thead>
              <tr>
                <th></th>
                <th>순금(g)</th>
                <th>공임 및 현금</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td class="ledger-label">거래 전 미수(A)</td>
                <td class="ledger-readonly">{{ receivableLedgerDetailBase.beforeWeight.toFixed(2) }}</td>
                <td class="ledger-readonly">₩ {{ formatPrice(receivableLedgerDetailBase.beforeAmount) }}</td>
              </tr>
              <tr>
                <td class="ledger-label">판매(B)</td>
                <td class="ledger-readonly">{{ (receivableLedgerDetailBase.newChargeWeight || 0).toFixed(2) }}</td>
                <td class="ledger-readonly">₩ {{ formatPrice(receivableLedgerDetailBase.newChargeAmount || 0) }}</td>
              </tr>
              <tr v-if="receivableLedgerDetailRow.isMostRecentPayment">
                <td class="ledger-label">결제(C)</td>
                <td>
                  <el-input-number v-model="getReceivableLedgerEditForm(receivableLedgerDetailRow).weight" :min="0" :precision="2" :step="0.1" size="small" style="width: 100%;" />
                </td>
                <td>
                  <el-input-number v-model="getReceivableLedgerEditForm(receivableLedgerDetailRow).amount" :min="0" :step="1000" size="small" style="width: 100%;" />
                </td>
              </tr>
              <tr v-else>
                <td class="ledger-label">결제(C)</td>
                <td class="ledger-readonly">{{ (receivableLedgerDetailRow.weight || 0).toFixed(2) }}</td>
                <td class="ledger-readonly">₩ {{ formatPrice(receivableLedgerDetailRow.amount) }}</td>
              </tr>
              <tr v-if="receivableLedgerDetailRow.isMostRecentPayment">
                <td class="ledger-label">할인(D)</td>
                <td>
                  <el-input-number v-model="getReceivableLedgerEditForm(receivableLedgerDetailRow).discountWeight" :min="0" :precision="2" :step="0.1" size="small" style="width: 100%;" />
                </td>
                <td>
                  <el-input-number v-model="getReceivableLedgerEditForm(receivableLedgerDetailRow).discount" :min="0" :step="1000" size="small" style="width: 100%;" />
                </td>
              </tr>
              <tr v-else>
                <td class="ledger-label">할인(D)</td>
                <td class="ledger-readonly">{{ (receivableLedgerDetailRow.discountWeight || 0).toFixed(2) }}</td>
                <td class="ledger-readonly">₩ {{ formatPrice(receivableLedgerDetailRow.discount) }}</td>
              </tr>
              <tr class="ledger-total-row">
                <td class="ledger-label">거래 후 미수(A+B-C-D)</td>
                <td class="ledger-readonly">{{ getReceivableLedgerAfter(receivableLedgerDetailRow).afterWeight.toFixed(2) }}</td>
                <td class="ledger-readonly">₩ {{ formatPrice(getReceivableLedgerAfter(receivableLedgerDetailRow).afterAmount) }}</td>
              </tr>
            </tbody>
          </table>
          <div v-if="receivableLedgerDetailRow.isMostRecentPayment" style="display: flex; justify-content: flex-end; margin-top: 0.625rem;">
            <el-button type="primary" size="small" :loading="receivableLedgerSaving" @click="saveReceivableLedgerEdit(receivableLedgerDetailRow)">저장</el-button>
          </div>
          </template>
        </div>
      </base-popup>
    </template>

    <template v-else>
      <settlement-history-filter
        :query="payableQuery"
        :is-mobile="isMobile"
        @update:query="Object.assign(payableQuery, $event)"
        @filter="handlePayableFilter"
        @reset="resetPayableQuery"
        @print-combined="printCombinedPayableStatement"
      />

      <table class="order-history-summary-table">
        <thead>
          <tr>
            <th></th>
            <th>순금(g)</th>
            <th>공임 및 현금</th>
            <th>금액합계</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td class="summary-label">총 판매</td>
            <td>{{ (payableSummary.totalChargeWeight || 0).toFixed(2) }}</td>
            <td>₩ {{ formatPrice(payableSummary.totalChargeAmount) }}</td>
            <td>0</td>
          </tr>
          <tr>
            <td class="summary-label">총 결제</td>
            <td>{{ (payableSummary.totalPaidWeight || 0).toFixed(2) }}</td>
            <td>₩ {{ formatPrice(payableSummary.totalPaidAmount) }}</td>
            <td>0</td>
          </tr>
          <tr>
            <td class="summary-label">미수금</td>
            <td>{{ payableOutstandingWeight.toFixed(2) }}</td>
            <td>₩ {{ formatPrice(payableOutstandingAmount) }}</td>
            <td>0</td>
          </tr>
        </tbody>
      </table>

      <el-card shadow="never">
        <base-table
          v-loading="payableLoading"
          :data="payableList"
          :total="payableTotal"
          v-model:page="payableQuery.page"
          v-model:page-size="payableQuery.pageSize"
          border
          row-key="id"
          style="width: 100%;"
          @change="fetchPayableList"
        >
          <el-table-column label="거래번호" width="100" align="center" prop="id" />
          <el-table-column label="거래처" width="180" align="center">
            <template #default="{row}">
              {{ row.logisticsCompanyName }}
            </template>
          </el-table-column>
          <el-table-column label="거래일자" width="160" align="center">
            <template #default="{row}">
              <span class="order-link" @click="openLedgerDetail(row)">{{ formatDate(row.createdAt) }}</span>
            </template>
          </el-table-column>
          <el-table-column label="거래 주문 수량" width="130" align="center">
            <template #default="{row}">
              {{ row.orderCount }}건
            </template>
          </el-table-column>
          <el-table-column label="정산 금액" width="150" align="right">
            <template #default="{row}">
              <span style="font-weight: bold; color: #67c23a;">₩ {{ formatPrice(row.amount) }}</span>
              <span v-if="row.weight > 0" style="color: #909399; font-size: 0.8125rem;"> ({{ row.weight.toFixed(2) }}g)</span>
            </template>
          </el-table-column>
          <el-table-column label="상태" width="100" align="center">
            <template #default="{row}">
              <el-tag v-if="row.isCancelled" type="info" size="small">취소됨</el-tag>
              <el-tag v-else-if="(row.outstandingChargeAmount || 0) > 0 || (row.outstandingChargeWeight || 0) > 0" type="warning" size="small">부분 정산</el-tag>
              <el-tag v-else type="success" size="small">완료</el-tag>
            </template>
          </el-table-column>
          <el-table-column label="미수 잔액" width="150" align="right">
            <template #default="{row}">
              <span v-if="!row.isCancelled && ((row.outstandingChargeAmount || 0) > 0 || (row.outstandingChargeWeight || 0) > 0)" style="color: #f56c6c; font-weight: bold;">
                ₩ {{ formatPrice(row.outstandingChargeAmount) }}
                <span v-if="row.outstandingChargeWeight > 0" style="color: #909399; font-size: 0.8125rem;"> ({{ row.outstandingChargeWeight.toFixed(2) }}g)</span>
              </span>
              <span v-else style="color: #c0c4cc;">-</span>
            </template>
          </el-table-column>
          <el-table-column label="메모" min-width="140" prop="memo" />
          <el-table-column label="작업" width="220" align="center" :fixed="!isMobile ? 'right' : false">
            <template #default="{row}">
              <div style="display: flex; gap: 0.375rem; justify-content: center;">
                <el-button size="small" @click="handleIssueStatement(row)">거래명세서</el-button>
                <el-button v-if="row.isCancelled || isHollowPayment(row)" size="small" type="danger" @click="handleDeletePayable(row)">삭제</el-button>
              </div>
            </template>
          </el-table-column>
        </base-table>
      </el-card>

      <payment-application-edit-dialog
        v-model="applicationEditDialogVisible"
        :record="editingApplication"
        :company-name="editingApplicationCompanyName"
        @saved="onApplicationEditSaved"
      />

      <base-popup v-model="ledgerDetailVisible" title="정산 상세" width="900px">
        <div v-if="ledgerDetailRow" class="order-detail-expand" v-loading="paymentApplicationsLoading[ledgerDetailRow.id]">
          <table class="ledger-table" style="margin-bottom: 1.25rem;">
            <thead>
              <tr>
                <th>거래번호</th>
                <th>발행처</th>
                <th>거래처</th>
                <th>거래일자</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td class="ledger-readonly">{{ ledgerDetailRow.id }}</td>
                <td class="ledger-readonly">{{ userStore.companyName || '-' }}</td>
                <td class="ledger-readonly">{{ ledgerDetailRow.logisticsCompanyName }}</td>
                <td class="ledger-readonly">{{ formatDate(ledgerDetailRow.createdAt) }}</td>
              </tr>
            </tbody>
          </table>
          <el-alert v-if="ledgerDetailRow.isCancelled" type="info" :closable="false" show-icon>
            이 정산은 취소되어 적용된 제품 내역이 없습니다. 취소 시 연결된 청구 금액이 원래대로 복구되었습니다.
          </el-alert>
          <template v-else>
          <h4>적용 제품 내역</h4>
          <base-table :data="paymentApplicationsData[ledgerDetailRow.id] || []" border size="small" style="width: 100%" row-key="chargeId">
            <el-table-column label="주문번호" width="180" align="center" prop="orderNo" />
            <el-table-column label="제품정보" min-width="280">
              <template #default="{row: item}">
                <div v-if="item.items && item.items.length > 0" class="product-info-list">
                  <div v-for="(p, idx) in item.items.slice(0, 3)" :key="idx" class="product-info-cell">
                    <el-image :src="p.photoUrl || defaultImage" fit="cover" class="product-thumb" style="width: 36px; height: 36px;" />
                    <div class="product-text">
                      <div class="product-name">
                        {{ p.productName || '-' }}
                        <span v-if="p.productNo" class="product-no-code">{{ p.productNo }}</span>
                      </div>
                      <div class="product-spec">
                        함량: {{ p.purity || '-' }} / 수량: {{ p.quantity }}개
                        <template v-if="p.color && p.color !== 'EMPTY'"> / 색상: {{ codeMap[p.color] || p.color }}</template>
                        <template v-if="p.size && p.size !== 'EMPTY'"> / 사이즈: {{ p.size }}</template>
                      </div>
                      <div v-if="p.memo" class="product-memo">메모: {{ p.memo }}</div>
                    </div>
                  </div>
                  <div v-if="item.items.length > 3" class="product-more">+{{ item.items.length - 3 }}건 더</div>
                </div>
                <span v-else>-</span>
              </template>
            </el-table-column>
            <el-table-column label="청구액" width="140" align="right">
              <template #default="{row: item}">
                <span style="font-weight: bold;">₩ {{ formatPrice(item.chargeAmount) }}</span>
              </template>
            </el-table-column>
            <el-table-column label="청구중량" width="120" align="right">
              <template #default="{row: item}">
                {{ (item.chargeWeight || 0).toFixed(2) }}g
              </template>
            </el-table-column>
            <el-table-column label="작업" width="180" align="center">
              <template #default="{row: item}">
                <div style="display: flex; gap: 0.375rem; justify-content: center;">
                  <el-button size="small" @click="handleIssueItemStatement(item, ledgerDetailRow)">거래명세서</el-button>
                  <el-button v-if="ledgerDetailRow.isMostRecentPayment" size="small" type="warning" @click="openApplicationEditDialog(item, ledgerDetailRow)">수정</el-button>
                </div>
              </template>
            </el-table-column>
          </base-table>

          <table v-if="paymentApplicationsData[ledgerDetailRow.id] && paymentApplicationsData[ledgerDetailRow.id].length > 0" class="purity-summary-table expand-ledger">
            <thead>
              <tr>
                <th>14K 합계(g)</th>
                <th>18K 합계(g)</th>
                <th>순금 합계(g)</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>{{ getPurityBreakdownForRow(ledgerDetailRow).p14.toFixed(2) }}</td>
                <td>{{ getPurityBreakdownForRow(ledgerDetailRow).p18.toFixed(2) }}</td>
                <td>{{ getPurityBreakdownForRow(ledgerDetailRow).pure.toFixed(2) }}</td>
              </tr>
            </tbody>
          </table>

          <table class="ledger-table expand-ledger">
            <thead>
              <tr>
                <th></th>
                <th>순금(g)</th>
                <th>공임 및 현금</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td class="ledger-label">거래 전 미지급(A)</td>
                <td class="ledger-readonly">{{ ledgerDetailBase.beforeWeight.toFixed(2) }}</td>
                <td class="ledger-readonly">₩ {{ formatPrice(ledgerDetailBase.beforeAmount) }}</td>
              </tr>
              <tr>
                <td class="ledger-label">청구(B)</td>
                <td class="ledger-readonly">{{ (ledgerDetailBase.newChargeWeight || 0).toFixed(2) }}</td>
                <td class="ledger-readonly">₩ {{ formatPrice(ledgerDetailBase.newChargeAmount || 0) }}</td>
              </tr>
              <tr v-if="ledgerDetailRow.isMostRecentPayment">
                <td class="ledger-label">결제(C)</td>
                <td>
                  <el-input-number v-model="getLedgerEditForm(ledgerDetailRow).weight" :min="0" :precision="2" :step="0.1" size="small" style="width: 100%;" />
                </td>
                <td>
                  <el-input-number v-model="getLedgerEditForm(ledgerDetailRow).amount" :min="0" :step="1000" size="small" style="width: 100%;" />
                </td>
              </tr>
              <tr v-else>
                <td class="ledger-label">결제(C)</td>
                <td class="ledger-readonly">{{ (ledgerDetailRow.weight || 0).toFixed(2) }}</td>
                <td class="ledger-readonly">₩ {{ formatPrice(ledgerDetailRow.amount) }}</td>
              </tr>
              <tr v-if="ledgerDetailRow.isMostRecentPayment">
                <td class="ledger-label">할인(D)</td>
                <td>
                  <el-input-number v-model="getLedgerEditForm(ledgerDetailRow).discountWeight" :min="0" :precision="2" :step="0.1" size="small" style="width: 100%;" />
                </td>
                <td>
                  <el-input-number v-model="getLedgerEditForm(ledgerDetailRow).discount" :min="0" :step="1000" size="small" style="width: 100%;" />
                </td>
              </tr>
              <tr v-else>
                <td class="ledger-label">할인(D)</td>
                <td class="ledger-readonly">{{ (ledgerDetailRow.discountWeight || 0).toFixed(2) }}</td>
                <td class="ledger-readonly">₩ {{ formatPrice(ledgerDetailRow.discount) }}</td>
              </tr>
              <tr class="ledger-total-row">
                <td class="ledger-label">거래 후 미지급(A+B-C-D)</td>
                <td class="ledger-readonly">{{ getLedgerAfter(ledgerDetailRow).afterWeight.toFixed(2) }}</td>
                <td class="ledger-readonly">₩ {{ formatPrice(getLedgerAfter(ledgerDetailRow).afterAmount) }}</td>
              </tr>
            </tbody>
          </table>
          <div v-if="ledgerDetailRow.isMostRecentPayment" style="display: flex; justify-content: flex-end; margin-top: 0.625rem;">
            <el-button type="primary" size="small" :loading="ledgerSaving[ledgerDetailRow.id]" @click="saveLedgerEdit(ledgerDetailRow)">저장</el-button>
          </div>
          </template>
        </div>
      </base-popup>
    </template>

  </div>
</template>

<script setup lang="ts">
import { ElMessage, ElMessageBox } from 'element-plus';
import { useMobile } from '@/hooks/useMobile';
import { ref, reactive, onMounted, computed } from 'vue';
import { getPayables, getPaymentApplications, getCompanySummaries, updatePayable, deletePayable, getLedgerBefore, getPayableOrderHistorySummary } from '@/api/payable';
import { getReceivables, getUserSummaries, updateReceivable, getLedgerBefore as getReceivableLedgerBefore, getReceivableOrderHistorySummary } from '@/api/receivable';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import useCodeStore from '@/store/modules/code';
import useUserStore from '@/store/modules/user';
import BaseTable from '@/components/BaseTable/index.vue';
import BasePopup from '@/components/BasePopup/index.vue';
import SettlementHistoryFilter from './components/SettlementHistoryFilter.vue';
import PaymentApplicationEditDialog from './components/PaymentApplicationEditDialog.vue';

const { isMobile } = useMobile();
const userStore = useUserStore();
const isMfg = computed(() => userStore.companyType === 'MFG');
// DCC's own settled-order history shares the same charge-centric Receivable branch below as
// RTL/admin (each scoped to their own data by the backend) - only MFG needs the separate
// Payable-based ledger, since it's a different pair of parties (MFG-DCC, not DCC-RTL).
const isPayableSide = computed(() => userStore.companyType === 'MFG');

const codeStore = useCodeStore();
const codeMap = computed(() => codeStore.codeMap);
const defaultImage = '/thumb_no_img.png';

const formatDate = (dateStr: string) => {
  if (!dateStr) return '';
  return parseTime(new Date(dateStr), '{y}-{m}-{d} {h}:{i}');
};

const end = new Date();
const start = new Date();
start.setTime(start.getTime() - 3600 * 1000 * 24 * 30);
const defaultStartDate = parseTime(start, '{y}-{m}-{d}');
const defaultEndDate = parseTime(end, '{y}-{m}-{d}');

// Amount and weight settle a charge together (either side fully paid clears both), so a
// literal 0 here can mean "this application's own weight pool never had to touch this
// charge, because the amount side already cleared it" - not "no weight was owed". When the
// charge is now fully settled but this application recorded 0 weight, show the charge's own
// weight instead so it doesn't read as unpaid. Shared by both the Payable and Receivable
// 정산 상세 popups below.
const getEffectiveAppliedWeight = (item: any) => {
  if (item.appliedWeight > 0) return item.appliedWeight;
  if ((item.chargeRemainingWeight || 0) <= 0 && (item.chargeRemainingAmount || 0) <= 0) {
    return item.chargeWeight || 0;
  }
  return item.appliedWeight || 0;
};

// ---- MFG/DCC (Payable) side: relocated from payable-management.vue's old
// transaction-log table, so both viewers get one canonical settlement-history view. ----

const payableList = ref<any[]>([]);
const payableTotal = ref(0);
const payableLoading = ref(false);
const payableQuery = reactive({
  page: 1,
  pageSize: 20,
  type: 'PAYMENT',
  companyId: undefined as number | undefined,
  startDate: defaultStartDate,
  endDate: defaultEndDate
});

const companySummaries = ref<any[]>([]);

const fetchCompanySummaries = async () => {
  try {
    const res: any = await getCompanySummaries({ page: 1, pageSize: 1000 });
    companySummaries.value = res.data.items || [];
  } catch (error) {
    console.error('Failed to fetch company summaries:', error);
  }
};

// The counterparty's current outstanding balance isn't on the PAYMENT row itself
// (each payment only knows its own amount/weight/discount), so the statement's
// "before/after" figures are sourced from the same company-summary list the
// company-level table on payable-management.vue already uses.
const getCompanyForRow = (row: any) => {
  const counterpartyId = isMfg.value ? row.logisticsCompanyId : row.manufacturerCompanyId;
  const found = companySummaries.value.find((c: any) => c.companyId === counterpartyId);
  if (found) return found;
  return {
    companyName: isMfg.value ? row.logisticsCompanyName : row.manufacturerCompanyName,
    totalOutstanding: 0,
    totalOutstandingWeight: 0,
    lastPaymentDate: null
  };
};

const fetchPayableList = async () => {
  payableLoading.value = true;
  try {
    const res: any = await getPayables({ ...payableQuery });
    payableList.value = res.data.items;
    payableTotal.value = res.data.totalCount;
  } catch (error) {
    console.error('Failed to fetch payable list:', error);
  } finally {
    payableLoading.value = false;
  }
};

// Lifetime running totals for the currently filtered company (or every company when
// no filter is applied) - deliberately not date-scoped, matching the same "총 is a
// running balance" convention the settlement worklists already use.
const payableSummary = reactive({
  totalChargeAmount: 0,
  totalChargeWeight: 0,
  totalPaidAmount: 0,
  totalPaidWeight: 0
});
const payableOutstandingAmount = computed(() => Math.max(0, (payableSummary.totalChargeAmount || 0) - (payableSummary.totalPaidAmount || 0)));
const payableOutstandingWeight = computed(() => Math.max(0, (payableSummary.totalChargeWeight || 0) - (payableSummary.totalPaidWeight || 0)));

const fetchPayableSummary = async () => {
  try {
    // page/pageSize must still be sent (backend binds them as non-nullable ints - the
    // request interceptor strips falsy params entirely, and omitting them 400s the request).
    const res: any = await getPayableOrderHistorySummary({ page: 1, pageSize: 1, companyId: payableQuery.companyId });
    Object.assign(payableSummary, res.data);
  } catch (error) {
    console.error('Failed to fetch payable order history summary:', error);
  }
};

const handlePayableFilter = () => {
  payableQuery.page = 1;
  fetchPayableList();
  fetchPayableSummary();
};

const resetPayableQuery = () => {
  payableQuery.companyId = undefined;
  payableQuery.startDate = defaultStartDate;
  payableQuery.endDate = defaultEndDate;
  handlePayableFilter();
};

const paymentApplicationsData = reactive<Record<number, any[]>>({});
const paymentApplicationsLoading = reactive<Record<number, boolean>>({});

const fetchPaymentApplications = async (paymentId: number) => {
  paymentApplicationsLoading[paymentId] = true;
  try {
    const res: any = await getPaymentApplications(paymentId);
    paymentApplicationsData[paymentId] = res.data;
  } catch (error) {
    console.error('Failed to fetch payment applications:', error);
  } finally {
    paymentApplicationsLoading[paymentId] = false;
  }
};

const ledgerDetailVisible = ref(false);
const ledgerDetailRow = ref<any>(null);
// getLedgerForRow approximates "before" from TODAY's company balance, which is only
// correct for the single most recent transaction - any earlier row silently bakes in
// every payment that happened after it too. This holds the real point-in-time balance
// (same getAccurateLedger lookup the 거래명세서 flow uses) so A/B here are accurate
// regardless of how many newer transactions exist for this row.
const ledgerDetailAccurate = ref<any>(null);

const ledgerDetailBase = computed(() => {
  if (ledgerDetailAccurate.value) return ledgerDetailAccurate.value;
  if (!ledgerDetailRow.value) return { beforeAmount: 0, beforeWeight: 0, newChargeAmount: 0, newChargeWeight: 0 };
  return { ...getLedgerForRow(ledgerDetailRow.value), newChargeAmount: 0, newChargeWeight: 0 };
});

const openLedgerDetail = async (row: any) => {
  ledgerDetailRow.value = row;
  ledgerDetailVisible.value = true;
  ledgerDetailAccurate.value = null;
  if (!row.isCancelled) {
    if (!paymentApplicationsData[row.id]) {
      await fetchPaymentApplications(row.id);
    }
    const rawLedger = await getAccurateLedger(row.id, row);
    ledgerDetailAccurate.value = buildSaleAdjustedLedger(rawLedger, paymentApplicationsData[row.id] || []);
  }
};

// company.totalOutstanding already reflects this payment's effect (it's the CURRENT
// balance), so adding this record's own amount/discount back gives the balance as it
// was before this payment was ever applied - shared by the print statement and the
// inline expand recap.
// p14/p18 are raw weight per purity (informational); pure is the true fine-gold total
// across ALL purities (14K/18K converted via their fine-gold ratio, 24K/PT as-is) -
// matches the "순금 합계(g)" column label, not just the raw 24K-only weight.
const getPurityBreakdownForRow = (row: any) => {
  const allItems = (paymentApplicationsData[row.id] || []).flatMap((app: any) => app.items || []);
  let p14 = 0;
  let p18 = 0;
  let pure = 0;
  allItems.forEach((item: any) => {
    const weight = (item.actualWeight || 0) * (item.quantity || 1);
    const purity = (item.purity || '').toUpperCase();
    if (purity.includes('14K')) p14 += weight;
    else if (purity.includes('18K')) p18 += weight;

    let ratio = 0;
    switch (purity) {
      case '14K': ratio = 0.6435; break;
      case '18K': ratio = 0.825; break;
      case '24K': ratio = 1.0; break;
      case 'PT': ratio = 0.95; break;
      default: ratio = purity.includes('PURE') ? 1.0 : 0;
    }
    pure += weight * ratio;
  });
  return { p14, p18, pure };
};

const getLedgerForRow = (record: any) => {
  const company = getCompanyForRow(record);
  const afterAmount = company.totalOutstanding || 0;
  const afterWeight = company.totalOutstandingWeight || 0;
  const beforeAmount = afterAmount + (record.amount || 0) + (record.discount || 0);
  const beforeWeight = afterWeight + (record.weight || 0) + (record.discountWeight || 0);
  return { company, afterAmount, afterWeight, beforeAmount, beforeWeight };
};

// getLedgerForRow's beforeAmount/beforeWeight only line up with reality for the single most
// recent payment - company.totalOutstanding is *today's* balance, so for any older record it
// silently bakes in every charge/payment that happened afterward too. This fetches the true
// point-in-time balance from the backend (which walks the full chronological ledger) and
// derives afterAmount/afterWeight from it, for statements where historical accuracy matters.
const getAccurateLedger = async (anchorId: number | undefined, record: any) => {
  const fallback = { ...getLedgerForRow(record), newChargeAmount: 0, newChargeWeight: 0 };
  if (!anchorId) return fallback;
  try {
    const res: any = await getLedgerBefore(anchorId);
    const beforeAmount = res.data.beforeAmount ?? fallback.beforeAmount;
    const beforeWeight = res.data.beforeWeight ?? fallback.beforeWeight;
    const newChargeAmount = res.data.newChargeAmount || 0;
    const newChargeWeight = res.data.newChargeWeight || 0;
    return {
      ...fallback,
      beforeAmount,
      beforeWeight,
      newChargeAmount,
      newChargeWeight,
      afterAmount: beforeAmount + newChargeAmount - (record.amount || 0) - (record.discount || 0),
      afterWeight: beforeWeight + newChargeWeight - (record.weight || 0) - (record.discountWeight || 0)
    };
  } catch (error) {
    console.error('Failed to fetch accurate ledger balance, falling back to approximation:', error);
    return fallback;
  }
};

// getAccurateLedger's newChargeAmount is "every charge that arrived since the last payment,"
// which over-counts whenever a payment doesn't cover everything that arrived in that window -
// e.g. a partial payment across several selected orders that (FIFO) only ends up touching the
// oldest one, leaving the rest completely untouched. 판매(B) must only ever show the charge(s)
// THIS payment's own applications actually paid toward, in their full original amount;
// whatever of the walk's total isn't attributable to those specific charges stays folded into
// A instead, so 거래 전 미수(A) correctly reads as "the rest of what's owed, unrelated to this
// receipt" rather than silently doubling as part of B too.
// `record` is optional - pass it (the payment's own static amount/discount) to also get
// afterAmount/afterWeight recomputed against the adjusted before/B. Omit it for the live-
// editable 정산 상세 popup, which recomputes "after" itself from the current edit form via
// getLedgerAfter instead of this fixed record.
const buildSaleAdjustedLedger = (ledger: any, applications: any[], record?: any) => {
  const saleAmount = applications.reduce((sum, a) => sum + (a.chargeAmount || 0), 0);
  const saleWeight = applications.reduce((sum, a) => sum + (a.chargeWeight || 0), 0);
  const totalBefore = (ledger.beforeAmount || 0) + (ledger.newChargeAmount || 0);
  const totalBeforeWeight = (ledger.beforeWeight || 0) + (ledger.newChargeWeight || 0);
  const beforeAmount = totalBefore - saleAmount;
  const beforeWeight = totalBeforeWeight - saleWeight;
  const adjusted = {
    ...ledger,
    beforeAmount,
    beforeWeight,
    newChargeAmount: saleAmount,
    newChargeWeight: saleWeight
  };
  if (record) {
    adjusted.afterAmount = beforeAmount + saleAmount - (record.amount || 0) - (record.discount || 0);
    adjusted.afterWeight = beforeWeight + saleWeight - (record.weight || 0) - (record.discountWeight || 0);
  }
  return adjusted;
};

// Inline-editable version of the expand ledger recap - C/D start out mirroring the
// record's own saved values, and 거래 후 미지급 recomputes live from the *edited* form,
// not the original row, so the user sees the effect of their change before saving.
const ledgerEditForm = reactive<Record<number, any>>({});
const ledgerSaving = reactive<Record<number, boolean>>({});

const getLedgerEditForm = (row: any) => {
  if (!ledgerEditForm[row.id]) {
    ledgerEditForm[row.id] = {
      weight: row.weight || 0,
      amount: row.amount || 0,
      discountWeight: row.discountWeight || 0,
      discount: row.discount || 0
    };
  }
  return ledgerEditForm[row.id];
};

const getLedgerAfter = (row: any) => {
  const base = ledgerDetailBase.value;
  const form = getLedgerEditForm(row);
  return {
    afterAmount: base.beforeAmount + (base.newChargeAmount || 0) - (form.amount || 0) - (form.discount || 0),
    afterWeight: base.beforeWeight + (base.newChargeWeight || 0) - (form.weight || 0) - (form.discountWeight || 0)
  };
};

const saveLedgerEdit = async (row: any) => {
  const form = getLedgerEditForm(row);
  ledgerSaving[row.id] = true;
  try {
    await updatePayable(row.id, {
      amount: form.amount,
      weight: form.weight,
      discount: form.discount,
      discountWeight: form.discountWeight,
      memo: row.memo
    });
    ElMessage.success('수정되었습니다.');
    delete ledgerEditForm[row.id];
    fetchPayableList();
    fetchCompanySummaries();
  } catch (error) {
    console.error('Failed to update payable:', error);
    ElMessage.error('수정에 실패했습니다.');
  } finally {
    ledgerSaving[row.id] = false;
  }
};

// Shared "임가공 거래 명세서" print template - same header/items/balance table structure
// as this file's own #print-area (used by printStatement for the retail order view), so
// the payable (MFG/DCC) side's statements look like the one document type, not two.
const paddedStatementItems = (items: any[], minCount = 6) => {
  const result = [...items];
  while (result.length < minCount) {
    result.push({ isDummy: true });
  }
  return result;
};

const buildStatementItemRows = (items: any[]) => {
  return paddedStatementItems(items || [], 6).map((item: any) => {
    if (item.isDummy) {
      return `<tr><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>`;
    }
    const cost = ((item.materialCost || 0) + (item.laborCost || 0)) * (item.quantity || 1);
    return `
      <tr>
        <td align="center">${item.productNo || '-'}</td>
        <td><div class="item-info-flex">${item.photoUrl ? `<img src="${item.photoUrl}" class="item-img" />` : ''}<span class="item-name">${item.productName || '-'}</span></div></td>
        <td align="center">${codeMap.value[item.purity] || item.purity || '-'}</td>
        <td align="right">${(item.actualWeight || 0).toFixed(3)}g</td>
        <td align="center">${item.quantity || 1}개</td>
        <td align="right">${formatPrice(cost)}</td>
      </tr>
    `;
  }).join('');
};

// 14K/18K raw totals plus the true fine-gold (24K-converted) total across every purity -
// same computation as getPurityBreakdownForRow, applied to a statement's own item list.
const buildStatementPurityRows = (items: any[]) => {
  let p14 = 0;
  let p18 = 0;
  let pure = 0;
  (items || []).forEach((item: any) => {
    const weight = (item.actualWeight || 0) * (item.quantity || 1);
    const purity = (item.purity || '').toUpperCase();
    if (purity.includes('14K')) p14 += weight;
    else if (purity.includes('18K')) p18 += weight;

    let ratio = 0;
    switch (purity) {
      case '14K': ratio = 0.6435; break;
      case '18K': ratio = 0.825; break;
      case '24K': ratio = 1.0; break;
      case 'PT': ratio = 0.95; break;
      default: ratio = purity.includes('PURE') ? 1.0 : 0;
    }
    pure += weight * ratio;
  });
  return `<tr><td align="right">${p14.toFixed(2)}g</td><td align="right">${p18.toFixed(2)}g</td><td align="right">${pure.toFixed(2)}g</td></tr>`;
};

const buildStatementBalanceRows = (ledger: any, record: any) => {
  const { company, beforeAmount, beforeWeight, afterAmount, afterWeight, newChargeAmount, newChargeWeight } = ledger;
  return `
    <tr><td class="row-label">최근 결제</td><td></td><td></td><td align="center" style="font-size: 9px;">${company.lastPaymentDate ? formatDate(company.lastPaymentDate) : '-'}</td></tr>
    <tr><td class="row-label">거래 전 미수(A)</td><td align="right">${beforeWeight.toFixed(2)}</td><td align="right">${formatPrice(beforeAmount)}</td><td></td></tr>
    <tr><td class="row-label">판매(B)</td><td align="right">${(newChargeWeight || 0).toFixed(2)}</td><td align="right">${formatPrice(newChargeAmount || 0)}</td><td></td></tr>
    <tr><td class="row-label">결제(C)</td><td align="right">${(record.weight || 0).toFixed(2)}</td><td align="right">${formatPrice(record.amount || 0)}</td><td></td></tr>
    <tr><td class="row-label">할인(D)</td><td align="right">${(record.discountWeight || 0).toFixed(2)}</td><td align="right">${formatPrice(record.discount || 0)}</td><td></td></tr>
    <tr class="after-balance-row"><td class="row-label"><strong>거래 후 미수<br/>(A+B-C-D)</strong></td><td align="right"><strong>${afterWeight.toFixed(2)}</strong></td><td align="right"><strong>${formatPrice(afterAmount)}</strong></td><td></td></tr>
  `;
};

// showProducts=false (used for a 미수금 관리 debt-collection deposit - see
// IsOverdueSettlement) omits the 주문내용/함량 순도 tables entirely, not just their rows -
// this transaction never introduced a new product charge, just collected an already-billed
// balance, so there's nothing product-related to itemize on its own receipt.
const buildStatementCopy = (copyLabel: string, opts: { companyTag: string; supplierName: string; date: string; transactionNo: string | number; itemRows: string; purityRows: string; balanceRows: string; showProducts?: boolean }) => `
  <div class="receipt-copy-block">
    <table class="receipt-header-table">
      <tr>
        <td class="header-title-cell">
          <span class="company-tag">[${opts.companyTag}]</span>
          <span class="main-title">임가공 거래 명세서</span>
          <span class="copy-badge ${copyLabel === '고객용' ? 'blue-badge' : ''}">(${copyLabel})</span>
        </td>
        <td class="supplier-info-cell" rowspan="2">
          <div>공급자: ${opts.supplierName}</div>
          <div>전 화: 051-633-1116</div>
          <div>팩 스: -</div>
        </td>
      </tr>
      <tr>
        <td class="header-meta-cell">
          <span>일자: ${opts.date}</span>
          <span class="meta-separator">|</span>
          <span>거래No: ${opts.transactionNo}</span>
        </td>
      </tr>
    </table>
    ${opts.showProducts === false ? '' : `
    <table class="receipt-items-table">
      <thead><tr><th width="12%">No</th><th width="42%">주문내용</th><th width="12%">함량</th><th width="13%">실중량</th><th width="10%">주문수량</th><th width="11%">공임비</th></tr></thead>
      <tbody>${opts.itemRows}</tbody>
    </table>
    <table class="receipt-purity-table">
      <thead><tr><th width="33%">14K 합계(g)</th><th width="33%">18K 합계(g)</th><th width="34%">순금(24K) 합계(g)</th></tr></thead>
      <tbody>${opts.purityRows}</tbody>
    </table>
    `}
    <table class="receipt-balance-table">
      <thead><tr><th width="30%"></th><th width="22%">순금(g)</th><th width="24%">공임 및 현금</th><th width="24%">금액 합계</th></tr></thead>
      <tbody>${opts.balanceRows}</tbody>
    </table>
  </div>
`;

const statementPrintStyles = `
  @page { size: A4 landscape; margin: 8mm; }
  body { font-family: 'Malgun Gothic', 'Noto Sans KR', sans-serif; color: #111; margin: 0; padding: 5px; background: #fff; }
  .receipt-double-container { display: flex; gap: 6mm; width: 100%; box-sizing: border-box; }
  .receipt-copy-block { flex: 1; min-width: 0; border: 1.5px solid #222; padding: 6px; background: #fff; box-sizing: border-box; }
  .receipt-header-table { width: 100%; border-collapse: collapse; margin-bottom: 6px; border: 1px solid #333; }
  .receipt-header-table td { padding: 4px 6px; border: 1px solid #333; vertical-align: top; }
  .header-title-cell { font-size: 12px; font-weight: bold; }
  .company-tag { color: #333; margin-right: 4px; }
  .main-title { font-size: 13px; font-weight: bold; letter-spacing: 0.5px; }
  .copy-badge { font-weight: bold; margin-left: 4px; color: #222; }
  .copy-badge.blue-badge { color: #1d4ed8; }
  .header-meta-cell { font-size: 10px; color: #444; background: #fafafa; }
  .meta-separator { margin: 0 4px; color: #aaa; }
  .supplier-info-cell { font-size: 10px; text-align: left; width: 35%; background: #fff; line-height: 1.3; }
  .receipt-items-table { width: 100%; border-collapse: collapse; margin-bottom: 8px; border: 1px solid #333; font-size: 10px; }
  .receipt-items-table th { background-color: #f2f2f2; border: 1px solid #333; padding: 4px 2px; font-weight: bold; text-align: center; color: #222; }
  .receipt-items-table td { border: 1px solid #333; padding: 3px 5px; vertical-align: middle; height: 25px; box-sizing: border-box; }
  .item-info-flex { display: flex; align-items: center; gap: 4px; }
  .item-img { width: 22px; height: 22px; object-fit: cover; border-radius: 2px; border: 1px solid #ccc; }
  .item-name { font-weight: 600; }
  .receipt-purity-table { width: 100%; border-collapse: collapse; margin-bottom: 8px; border: 1px solid #333; font-size: 10px; }
  .receipt-purity-table th { background-color: #f2f2f2; border: 1px solid #333; padding: 4px; font-weight: bold; text-align: center; color: #222; }
  .receipt-purity-table td { border: 1px solid #333; padding: 3px 5px; }
  .receipt-balance-table { width: 100%; border-collapse: collapse; border: 1px solid #333; font-size: 10px; }
  .receipt-balance-table th { background-color: #f2f2f2; border: 1px solid #333; padding: 4px; font-weight: bold; text-align: center; color: #222; }
  .receipt-balance-table td { border: 1px solid #333; padding: 3px 5px; }
  .receipt-balance-table .row-label { background-color: #fafafa; font-weight: 600; text-align: left; }
  .receipt-balance-table .after-balance-row td { background-color: #f8f9fa; font-weight: bold; }
`;

const openStatementPrintWindow = (title: string, bodyHtml: string) => {
  const printWindow = window.open('', '_blank');
  if (!printWindow) return;
  printWindow.document.write(`
    <html>
      <head><title>${title}</title><style>${statementPrintStyles}</style></head>
      <body>
        <div class="receipt-double-container">${bodyHtml}</div>
        <script>window.onload = () => { window.print(); setTimeout(() => window.close(), 500); };<\/script>
      </body>
    </html>
  `);
  printWindow.document.close();
};

// 거래명세서 for the whole payment - lists every order/product this payment covered
// (fetching the applications first if the row hasn't been expanded yet) plus the same
// A/B/C/D company ledger used everywhere else on this page.
const handleIssueStatement = async (record: any) => {
  if (!paymentApplicationsData[record.id]) {
    await fetchPaymentApplications(record.id);
  }
  const applications = paymentApplicationsData[record.id] || [];
  const allItems = applications.flatMap((app: any) => app.items || []);

  const rawLedger = await getAccurateLedger(record.id, record);
  const ledger = buildSaleAdjustedLedger(rawLedger, applications, record);
  const supplierName = !isMfg.value ? ledger.company.companyName : userStore.companyName || '-';
  const payerName = !isMfg.value ? userStore.companyName || '-' : ledger.company.companyName;

  const itemRows = buildStatementItemRows(allItems);
  const purityRows = buildStatementPurityRows(allItems);
  const balanceRows = buildStatementBalanceRows(ledger, record);

  const bodyHtml = ['고객용', '보관용'].map((label) => buildStatementCopy(label, {
    companyTag: payerName,
    supplierName,
    date: formatDate(record.createdAt),
    transactionNo: record.id,
    itemRows,
    purityRows,
    balanceRows
  })).join('');

  openStatementPrintWindow(`정산 명세서 - ${payerName}`, bodyHtml);
};

// 일괄 거래명세서 - same "임가공 거래 명세서" template as handleIssueStatement, but summed
// across every payment currently listed (respecting the active filter), with every
// applied order/product line from every payment appended into one items table.
const printCombinedPayableStatement = async () => {
  if (!payableList.value || payableList.value.length === 0) {
    ElMessage.warning('출력할 거래 내역이 없습니다.');
    return;
  }

  // payableList is whatever page is currently on screen (20 by default) - a "combined"
  // statement has to cover every record matching the active filter/date range, not just
  // the visible page, so re-fetch the full unpaginated set instead of reusing payableList.
  let combinedList = payableList.value;
  try {
    const fullRes: any = await getPayables({ ...payableQuery, page: 1, pageSize: 10000 });
    combinedList = fullRes.data.items || payableList.value;
  } catch (error) {
    console.error('Failed to fetch full payable list for combined statement, falling back to current page:', error);
  }

  await Promise.all(combinedList.map(async (p: any) => {
    if (!paymentApplicationsData[p.id]) {
      await fetchPaymentApplications(p.id);
    }
  }));

  const company = getCompanyForRow(combinedList[0]);

  let totalAmount = 0;
  let totalWeight = 0;
  let totalDiscount = 0;
  let totalDiscountWeight = 0;
  const allItems: any[] = [];
  // A charge touched by more than one payment in this batch (legacy pre-merge data) would
  // otherwise have its full chargeAmount counted once per payment - merge by chargeId first
  // so 판매(B) below counts each charge exactly once, regardless of how many payments in the
  // batch contributed to it.
  const applicationsByCharge = new Map<number, any>();

  combinedList.forEach((p: any) => {
    totalAmount += p.amount || 0;
    totalWeight += p.weight || 0;
    totalDiscount += p.discount || 0;
    totalDiscountWeight += p.discountWeight || 0;
    (paymentApplicationsData[p.id] || []).forEach((app: any) => {
      allItems.push(...(app.items || []));
      const existing = applicationsByCharge.get(app.chargeId);
      if (existing) {
        existing.appliedAmount += app.appliedAmount || 0;
        existing.appliedWeight += app.appliedWeight || 0;
      } else {
        applicationsByCharge.set(app.chargeId, { ...app });
      }
    });
  });
  const mergedApplications = Array.from(applicationsByCharge.values());

  const pseudoRecord = { weight: totalWeight, amount: totalAmount, discountWeight: totalDiscountWeight, discount: totalDiscount };

  // "Before" for a combined statement means before the *earliest* payment in the batch -
  // every payment after that is already folded into the combined C/D totals above.
  const earliestRecord = [...combinedList].sort((a: any, b: any) => {
    const dateDiff = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
    return dateDiff !== 0 ? dateDiff : a.id - b.id;
  })[0];

  let beforeAmount = (company.totalOutstanding || 0) + totalAmount + totalDiscount;
  let beforeWeight = (company.totalOutstandingWeight || 0) + totalWeight + totalDiscountWeight;
  let newChargeAmount = 0;
  let newChargeWeight = 0;
  if (earliestRecord?.id) {
    try {
      const res: any = await getLedgerBefore(earliestRecord.id);
      beforeAmount = res.data.beforeAmount;
      beforeWeight = res.data.beforeWeight;
      newChargeAmount = res.data.newChargeAmount || 0;
      newChargeWeight = res.data.newChargeWeight || 0;
    } catch (error) {
      console.error('Failed to fetch accurate ledger balance, falling back to approximation:', error);
    }
  }

  const rawLedger = { company, beforeAmount, beforeWeight, newChargeAmount, newChargeWeight };
  const ledger = buildSaleAdjustedLedger(rawLedger, mergedApplications, pseudoRecord);

  const supplierName = !isMfg.value ? company.companyName : userStore.companyName || '-';
  const payerName = !isMfg.value ? userStore.companyName || '-' : company.companyName;

  const dateTitle = payableQuery.startDate && payableQuery.endDate
    ? (payableQuery.startDate === payableQuery.endDate ? payableQuery.startDate : `${payableQuery.startDate} ~ ${payableQuery.endDate}`)
    : '전체 기간';

  const itemRows = buildStatementItemRows(allItems);
  const purityRows = buildStatementPurityRows(allItems);
  const balanceRows = buildStatementBalanceRows(ledger, pseudoRecord);

  const bodyHtml = ['고객용', '보관용'].map((label) => buildStatementCopy(label, {
    companyTag: payerName,
    supplierName,
    date: dateTitle,
    transactionNo: `${combinedList.length}건`,
    itemRows,
    purityRows,
    balanceRows
  })).join('');

  openStatementPrintWindow(`일괄 정산 명세서 - ${payerName}`, bodyHtml);
};

// Per-order receipt for a single row inside the expand's product breakdown - same
// "임가공 거래 명세서" template as handleIssueStatement, but the items table is scoped to
// just this order's own products instead of every order the payment covered.
const handleIssueItemStatement = async (item: any, record: any) => {
  // 결제(C)/할인(D) must reflect only what THIS order's application received, not the full
  // payment's totals - a single payment can cover multiple orders, and this receipt is
  // scoped to just one of them. The ledger-before lookup still anchors on the payment's own
  // chronological position (record.id), since that's when the money actually moved.
  const itemRecord = {
    amount: item.appliedAmount || 0,
    weight: getEffectiveAppliedWeight(item),
    discount: 0,
    discountWeight: 0,
    createdAt: record.createdAt
  };
  const rawLedger = await getAccurateLedger(record.id, itemRecord);
  const ledger = buildSaleAdjustedLedger(rawLedger, [item], itemRecord);
  const supplierName = !isMfg.value ? ledger.company.companyName : userStore.companyName || '-';
  const payerName = !isMfg.value ? userStore.companyName || '-' : ledger.company.companyName;

  const itemRows = buildStatementItemRows(item.items || []);
  const purityRows = buildStatementPurityRows(item.items || []);
  const balanceRows = buildStatementBalanceRows(ledger, itemRecord);

  const bodyHtml = ['고객용', '보관용'].map((label) => buildStatementCopy(label, {
    companyTag: payerName,
    supplierName,
    date: formatDate(record.createdAt),
    transactionNo: item.chargeId,
    itemRows,
    purityRows,
    balanceRows,
    showProducts: !record.isOverdueSettlement
  })).join('');

  openStatementPrintWindow(`정산 영수증 - ${item.orderNo}`, bodyHtml);
};

// A payment that never touched any charge and carries no amount at all (a leftover-overpay
// shell from before ProcessPaymentAsync always carried its own Amount/Discount) has nothing
// to reverse, so it's safe to delete directly without going through cancel first.
const isHollowPayment = (row: any) => {
  return (row.orderCount || 0) === 0 && (row.amount || 0) === 0 && (row.discount || 0) === 0 && (row.weight || 0) === 0 && (row.discountWeight || 0) === 0;
};

const handleDeletePayable = (record: any) => {
  ElMessageBox.confirm('이 취소된 정산 내역을 완전히 삭제하시겠습니까? 삭제 후 되돌릴 수 없습니다.', '정산 내역 삭제', {
    confirmButtonText: '삭제',
    cancelButtonText: '취소',
    type: 'warning'
  }).then(async () => {
    try {
      await deletePayable(record.id);
      ElMessage.success('삭제되었습니다.');
      fetchPayableList();
    } catch (error) {
      console.error('Failed to delete payable:', error);
      ElMessage.error('삭제에 실패했습니다.');
    }
  }).catch(() => {});
};

const applicationEditDialogVisible = ref(false);
const editingApplication = ref<any>(null);
const editingApplicationCompanyName = ref('');
const editingApplicationPaymentId = ref<number | null>(null);

const openApplicationEditDialog = (item: any, record: any) => {
  editingApplication.value = item;
  editingApplicationCompanyName.value = getCompanyForRow(record).companyName;
  editingApplicationPaymentId.value = record.id;
  applicationEditDialogVisible.value = true;
};

const onApplicationEditSaved = () => {
  if (editingApplicationPaymentId.value != null) {
    delete paymentApplicationsData[editingApplicationPaymentId.value];
    fetchPaymentApplications(editingApplicationPaymentId.value);
  }
  fetchPayableList();
  fetchCompanySummaries();
};

// ---- RTL/DCC (Receivable) side: same charge-centric 거래별 보기 pattern as the Payable
// side above (one row per deposit, 상세 popup with the same simple A/B/C/D ledger) - each
// deposit already carries its own appliedCharges (with full per-order item lists), so unlike
// the Payable side there's no separate "fetch applications for this row" round-trip needed.

const receivableList = ref<any[]>([]);
const receivableTotal = ref(0);
const receivableListLoading = ref(false);
const receivableQuery = reactive({
  page: 1,
  pageSize: 20,
  type: 'DEPOSIT',
  companyId: undefined as number | undefined,
  productName: '',
  startDate: defaultStartDate,
  endDate: defaultEndDate
});

const fetchReceivableList = async () => {
  receivableListLoading.value = true;
  try {
    const res: any = await getReceivables({ ...receivableQuery });
    receivableList.value = res.data.items;
    receivableTotal.value = res.data.totalCount;
  } catch (error) {
    console.error('Failed to fetch receivable list:', error);
  } finally {
    receivableListLoading.value = false;
  }
};

// Lifetime running totals for the currently filtered company (or every company when
// no filter is applied) - deliberately not date-scoped, matching the same "총 is a
// running balance" convention the settlement worklists already use.
const receivableSummary = reactive({
  totalChargeAmount: 0,
  totalChargeWeight: 0,
  totalPaidAmount: 0,
  totalPaidWeight: 0
});
const receivableOutstandingAmount = computed(() => Math.max(0, (receivableSummary.totalChargeAmount || 0) - (receivableSummary.totalPaidAmount || 0)));
const receivableOutstandingWeight = computed(() => Math.max(0, (receivableSummary.totalChargeWeight || 0) - (receivableSummary.totalPaidWeight || 0)));

const fetchReceivableSummary = async () => {
  try {
    const res: any = await getReceivableOrderHistorySummary({ page: 1, pageSize: 1, companyId: receivableQuery.companyId });
    Object.assign(receivableSummary, res.data);
  } catch (error) {
    console.error('Failed to fetch receivable order history summary:', error);
  }
};

const handleReceivableFilter = () => {
  receivableQuery.page = 1;
  fetchReceivableList();
  fetchReceivableSummary();
};

const resetReceivableQuery = () => {
  receivableQuery.companyId = undefined;
  receivableQuery.productName = '';
  receivableQuery.startDate = defaultStartDate;
  receivableQuery.endDate = defaultEndDate;
  handleReceivableFilter();
};

const userSummaries = ref<any[]>([]);

const fetchUserSummaries = async () => {
  try {
    const res: any = await getUserSummaries({ page: 1, pageSize: 1000 });
    userSummaries.value = res.data.items || [];
  } catch (error) {
    console.error('Failed to fetch user summaries:', error);
  }
};

// The counterparty's current outstanding balance isn't on the DEPOSIT row itself, so the
// statement's "before/after" figures are sourced from the same user-summary list 미수금
// 관리 already uses - mirrors the Payable side's getCompanyForRow.
const getUserForReceivableRow = (row: any) => {
  const found = userSummaries.value.find((u: any) => u.userId === row.userId);
  if (found) {
    return {
      companyName: found.companyName || found.userDisplayName,
      totalOutstanding: found.totalReceivable || 0,
      totalOutstandingWeight: found.totalReceivableWeight || 0,
      lastPaymentDate: found.lastPaymentDate
    };
  }
  return {
    companyName: row.companyName || row.userDisplayName,
    totalOutstanding: 0,
    totalOutstandingWeight: 0,
    lastPaymentDate: null
  };
};

const getLedgerForReceivableRow = (record: any) => {
  const company = getUserForReceivableRow(record);
  const afterAmount = company.totalOutstanding || 0;
  const afterWeight = company.totalOutstandingWeight || 0;
  const beforeAmount = afterAmount + (record.amount || 0) + (record.discount || 0);
  const beforeWeight = afterWeight + (record.weight || 0) + (record.discountWeight || 0);
  return { company, afterAmount, afterWeight, beforeAmount, beforeWeight };
};

// Same reasoning as the Payable side's getAccurateLedger - company.totalOutstanding is
// *today's* balance, only valid as "before" for the single most recent deposit.
const getAccurateReceivableLedger = async (anchorId: number | undefined, record: any) => {
  const fallback = { ...getLedgerForReceivableRow(record), newChargeAmount: 0, newChargeWeight: 0 };
  if (!anchorId) return fallback;
  try {
    const res: any = await getReceivableLedgerBefore(anchorId);
    const beforeAmount = res.data.beforeAmount ?? fallback.beforeAmount;
    const beforeWeight = res.data.beforeWeight ?? fallback.beforeWeight;
    const newChargeAmount = res.data.newChargeAmount || 0;
    const newChargeWeight = res.data.newChargeWeight || 0;
    return {
      ...fallback,
      beforeAmount,
      beforeWeight,
      newChargeAmount,
      newChargeWeight,
      afterAmount: beforeAmount + newChargeAmount - (record.amount || 0) - (record.discount || 0),
      afterWeight: beforeWeight + newChargeWeight - (record.weight || 0) - (record.discountWeight || 0)
    };
  } catch (error) {
    console.error('Failed to fetch accurate receivable ledger balance, falling back to approximation:', error);
    return fallback;
  }
};

const getPurityBreakdownForReceivableRow = (row: any) => {
  const allItems = (row.appliedCharges || []).flatMap((app: any) => app.items || []);
  let p14 = 0;
  let p18 = 0;
  let pure = 0;
  allItems.forEach((item: any) => {
    const weight = (item.actualWeight || 0) * (item.quantity || 1);
    const purity = (item.purity || '').toUpperCase();
    if (purity.includes('14K')) p14 += weight;
    else if (purity.includes('18K')) p18 += weight;

    let ratio = 0;
    switch (purity) {
      case '14K': ratio = 0.6435; break;
      case '18K': ratio = 0.825; break;
      case '24K': ratio = 1.0; break;
      case 'PT': ratio = 0.95; break;
      default: ratio = purity.includes('PURE') ? 1.0 : 0;
    }
    pure += weight * ratio;
  });
  return { p14, p18, pure };
};

const receivableLedgerDetailVisible = ref(false);
const receivableLedgerDetailRow = ref<any>(null);
const receivableLedgerDetailAccurate = ref<any>(null);
const receivableLedgerLoading = ref(false);

const receivableLedgerDetailBase = computed(() => {
  if (receivableLedgerDetailAccurate.value) return receivableLedgerDetailAccurate.value;
  if (!receivableLedgerDetailRow.value) return { beforeAmount: 0, beforeWeight: 0, newChargeAmount: 0, newChargeWeight: 0 };
  return { ...getLedgerForReceivableRow(receivableLedgerDetailRow.value), newChargeAmount: 0, newChargeWeight: 0 };
});

const openReceivableLedgerDetail = async (row: any) => {
  receivableLedgerDetailRow.value = row;
  receivableLedgerDetailVisible.value = true;
  receivableLedgerDetailAccurate.value = null;
  if (!row.isCancelled) {
    receivableLedgerLoading.value = true;
    try {
      const rawLedger = await getAccurateReceivableLedger(row.id, row);
      receivableLedgerDetailAccurate.value = buildSaleAdjustedLedger(rawLedger, row.appliedCharges || []);
    } finally {
      receivableLedgerLoading.value = false;
    }
  }
};

// Inline-editable version of the ledger recap, same pattern as the Payable side's
// getLedgerEditForm/getLedgerAfter/saveLedgerEdit - Receivable has no weight-side discount
// field, so 할인(D) here is amount-only.
const receivableLedgerEditForm = reactive<Record<number, any>>({});
const receivableLedgerSaving = ref(false);

const getReceivableLedgerEditForm = (row: any) => {
  if (!receivableLedgerEditForm[row.id]) {
    receivableLedgerEditForm[row.id] = {
      weight: row.weight || 0,
      amount: row.amount || 0,
      discount: row.discount || 0,
      discountWeight: row.discountWeight || 0
    };
  }
  return receivableLedgerEditForm[row.id];
};

const getReceivableLedgerAfter = (row: any) => {
  const base = receivableLedgerDetailBase.value;
  const form = getReceivableLedgerEditForm(row);
  return {
    afterAmount: base.beforeAmount + (base.newChargeAmount || 0) - (form.amount || 0) - (form.discount || 0),
    afterWeight: base.beforeWeight + (base.newChargeWeight || 0) - (form.weight || 0) - (form.discountWeight || 0)
  };
};

const saveReceivableLedgerEdit = async (row: any) => {
  const form = getReceivableLedgerEditForm(row);
  receivableLedgerSaving.value = true;
  try {
    await updateReceivable(row.id, {
      amount: form.amount,
      weight: form.weight,
      discount: form.discount,
      discountWeight: form.discountWeight,
      memo: row.memo,
      settlementMethod: row.settlementMethod
    });
    ElMessage.success('수정되었습니다.');
    delete receivableLedgerEditForm[row.id];
    await fetchReceivableList();
    const refreshed = receivableList.value.find((r: any) => r.id === row.id);
    if (refreshed) {
      receivableLedgerDetailRow.value = refreshed;
      const rawLedger = await getAccurateReceivableLedger(refreshed.id, refreshed);
      receivableLedgerDetailAccurate.value = buildSaleAdjustedLedger(rawLedger, refreshed.appliedCharges || []);
    }
    fetchUserSummaries();
  } catch (error) {
    console.error('Failed to update receivable:', error);
    ElMessage.error('수정에 실패했습니다.');
  } finally {
    receivableLedgerSaving.value = false;
  }
};

const handleIssueReceivableStatement = async (record: any) => {
  const allItems = (record.appliedCharges || []).flatMap((app: any) => app.items || []);

  const rawLedger = await getAccurateReceivableLedger(record.id, record);
  const ledger = buildSaleAdjustedLedger(rawLedger, record.appliedCharges || [], record);
  const supplierName = userStore.companyName || '-';
  const payerName = ledger.company.companyName || '-';

  const itemRows = buildStatementItemRows(allItems);
  const purityRows = buildStatementPurityRows(allItems);
  const balanceRows = buildStatementBalanceRows(ledger, record);

  const bodyHtml = ['고객용', '보관용'].map((label) => buildStatementCopy(label, {
    companyTag: payerName,
    supplierName,
    date: formatDate(record.createdAt),
    transactionNo: record.id,
    itemRows,
    purityRows,
    balanceRows,
    showProducts: !record.isOverdueSettlement
  })).join('');

  openStatementPrintWindow(`정산 명세서 - ${payerName}`, bodyHtml);
};

const handleIssueReceivableItemStatement = async (item: any, record: any) => {
  const itemRecord = {
    amount: item.appliedAmount || 0,
    weight: getEffectiveAppliedWeight(item),
    discount: 0,
    createdAt: record.createdAt
  };
  const rawLedger = await getAccurateReceivableLedger(record.id, itemRecord);
  const ledger = buildSaleAdjustedLedger(rawLedger, [item], itemRecord);
  const supplierName = userStore.companyName || '-';
  const payerName = ledger.company.companyName || '-';

  const itemRows = buildStatementItemRows(item.items || []);
  const purityRows = buildStatementPurityRows(item.items || []);
  const balanceRows = buildStatementBalanceRows(ledger, itemRecord);

  const bodyHtml = ['고객용', '보관용'].map((label) => buildStatementCopy(label, {
    companyTag: payerName,
    supplierName,
    date: formatDate(record.createdAt),
    transactionNo: item.chargeId,
    itemRows,
    purityRows,
    balanceRows,
    showProducts: !record.isOverdueSettlement
  })).join('');

  openStatementPrintWindow(`정산 영수증 - ${item.orderNo}`, bodyHtml);
};

const printCombinedReceivableStatement = async () => {
  if (!receivableList.value || receivableList.value.length === 0) {
    ElMessage.warning('출력할 거래 내역이 없습니다.');
    return;
  }

  let combinedList = receivableList.value;
  try {
    const fullRes: any = await getReceivables({ ...receivableQuery, page: 1, pageSize: 10000 });
    combinedList = fullRes.data.items || receivableList.value;
  } catch (error) {
    console.error('Failed to fetch full receivable list for combined statement, falling back to current page:', error);
  }

  const company = getUserForReceivableRow(combinedList[0]);

  let totalAmount = 0;
  let totalWeight = 0;
  let totalDiscount = 0;
  let totalDiscountWeight = 0;
  const allItems: any[] = [];
  const applicationsByCharge = new Map<number, any>();

  combinedList.forEach((p: any) => {
    totalAmount += p.amount || 0;
    totalWeight += p.weight || 0;
    totalDiscount += p.discount || 0;
    totalDiscountWeight += p.discountWeight || 0;
    (p.appliedCharges || []).forEach((app: any) => {
      allItems.push(...(app.items || []));
      const existing = applicationsByCharge.get(app.chargeId);
      if (existing) {
        existing.appliedAmount += app.appliedAmount || 0;
        existing.appliedWeight += app.appliedWeight || 0;
      } else {
        applicationsByCharge.set(app.chargeId, { ...app });
      }
    });
  });
  const mergedApplications = Array.from(applicationsByCharge.values());

  const pseudoRecord = { weight: totalWeight, amount: totalAmount, discount: totalDiscount, discountWeight: totalDiscountWeight };

  const earliestRecord = [...combinedList].sort((a: any, b: any) => {
    const dateDiff = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
    return dateDiff !== 0 ? dateDiff : a.id - b.id;
  })[0];

  let beforeAmount = (company.totalOutstanding || 0) + totalAmount + totalDiscount;
  let beforeWeight = (company.totalOutstandingWeight || 0) + totalWeight + totalDiscountWeight;
  let newChargeAmount = 0;
  let newChargeWeight = 0;
  if (earliestRecord?.id) {
    try {
      const res: any = await getReceivableLedgerBefore(earliestRecord.id);
      beforeAmount = res.data.beforeAmount;
      beforeWeight = res.data.beforeWeight;
      newChargeAmount = res.data.newChargeAmount || 0;
      newChargeWeight = res.data.newChargeWeight || 0;
    } catch (error) {
      console.error('Failed to fetch accurate receivable ledger balance, falling back to approximation:', error);
    }
  }

  const rawLedger = { company, beforeAmount, beforeWeight, newChargeAmount, newChargeWeight };
  const ledger = buildSaleAdjustedLedger(rawLedger, mergedApplications, pseudoRecord);

  const supplierName = userStore.companyName || '-';
  const payerName = company.companyName || '-';

  const dateTitle = receivableQuery.startDate && receivableQuery.endDate
    ? (receivableQuery.startDate === receivableQuery.endDate ? receivableQuery.startDate : `${receivableQuery.startDate} ~ ${receivableQuery.endDate}`)
    : '전체 기간';

  const itemRows = buildStatementItemRows(allItems);
  const purityRows = buildStatementPurityRows(allItems);
  const balanceRows = buildStatementBalanceRows(ledger, pseudoRecord);

  const bodyHtml = ['고객용', '보관용'].map((label) => buildStatementCopy(label, {
    companyTag: payerName,
    supplierName,
    date: dateTitle,
    transactionNo: `${combinedList.length}건`,
    itemRows,
    purityRows,
    balanceRows
  })).join('');

  openStatementPrintWindow(`일괄 정산 명세서 - ${payerName}`, bodyHtml);
};

onMounted(() => {
  if (isPayableSide.value) {
    fetchCompanySummaries();
    fetchPayableList();
    fetchPayableSummary();
  } else {
    fetchUserSummaries();
    fetchReceivableList();
    fetchReceivableSummary();
  }
});
</script>

<style lang="scss" scoped>
@import "./SettlementHistoryStyles.scss";
</style>

