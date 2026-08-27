<template>
<div class="settlement-history-container app-container">

    <template v-if="!isPayableSide">
      <el-row :gutter="20" class="summary-cards" v-loading="summaryLoading">
        <el-col :xs="24" :sm="8" class="mb-4 sm:mb-0">
          <el-card shadow="hover" class="summary-card">
            <template #header><div class="card-header"><span>{{ $t('admin.settlement.summary.totalActualWeight') }}</span></div></template>
            <div class="card-value">{{ summaryData.totalActualWeight.toFixed(2) }}g</div>
          </el-card>
        </el-col>
        <el-col :xs="24" :sm="8" class="mb-4 sm:mb-0">
          <el-card shadow="hover" class="summary-card gold">
            <template #header><div class="card-header"><span>{{ $t('admin.settlement.summary.totalFineGold') }}</span></div></template>
            <div class="card-value">{{ summaryData.totalFineGold.toFixed(2) }}g</div>
          </el-card>
        </el-col>
        <el-col :xs="24" :sm="8">
          <el-card shadow="hover" class="summary-card settle">
            <template #header><div class="card-header"><span>{{ $t('admin.settlement.summary.totalAmount') }}</span></div></template>
            <div class="card-value">₩ {{ formatPrice(summaryData.totalSettlementAmount) }}</div>
          </el-card>
        </el-col>
      </el-row>

      <settlement-history-filter
        :query="listQuery"
        :is-mobile="isMobile"
        company-category="RTL"
        company-label="소매점"
        @update:query="Object.assign(listQuery, $event)"
        @filter="handleFilter"
        @reset="resetQuery"
        @print-combined="printCombinedStatement"
        style="margin-top: 1.25rem;"
      />

      <el-card shadow="never" style="margin-top: 1.25rem;">
        <base-table
          v-loading="listLoading"
          :data="list"
          border
          fit
          highlight-current-row
          style="width: 100%"
          row-key="id"
        >
          <el-table-column type="expand">
            <template #default="{row}">
              <div class="order-detail-expand">
                <h4>{{ $t('admin.settlement.table.expandTitle') }}</h4>
                <base-table :data="flattenOrderItems(row.orderItems)" border size="small" style="width: 100%">
                  <el-table-column :label="$t('orderDetail.headers.productInfo')" min-width="250" prop="productName" :excel-formatter="productItemInfoFormatter">
                    <template #default="item">
                      <div class="product-info-cell" :style="{ paddingLeft: item.row.depth * 20 + 'px' }">
                        <el-image :src="item.row.photoUrl || defaultImage" fit="cover" class="product-thumb" style="width: 35px; height: 35px;" />
                        <div class="product-text">
                          <div class="product-name">{{ item.row.productName || item.row.productSetTitle }}</div>
                          <div class="product-no">{{ item.row.productNo }}</div>
                          <div class="product-options" v-if="item.row.purity || item.row.color">
                            <el-tag size="small" type="info" effect="plain" v-if="item.row.purity">{{ codeMap[item.row.purity] || item.row.purity }}</el-tag>
                            <el-tag size="small" type="info" effect="plain" v-if="item.row.color && item.row.color !== 'EMPTY'" style="margin-left: 0.3125rem;">{{ codeMap[item.row.color] || item.row.color }}</el-tag>
                          </div>
                        </div>
                      </div>
                    </template>
                  </el-table-column>
                  <el-table-column :label="$t('orderDetail.headers.actualWeight')" width="90" align="center" prop="actualWeight" :excel-formatter="(row) => row.actualWeight ? row.actualWeight + 'g' : '-'">
                    <template #default="item">
                      <span>{{ item.row.actualWeight ? item.row.actualWeight + 'g' : '-' }}</span>
                    </template>
                  </el-table-column>
                  <el-table-column :label="$t('productDetail.labels.purity')" width="80" align="center" prop="purity" />
                  <el-table-column :label="$t('admin.settlement.table.fineGold')" width="100" align="center" prop="fineGold" :excel-formatter="(row) => calculatePurityWeight(row.actualWeight, row.purity) + 'g'">
                    <template #default="item">
                      <span>{{ calculatePurityWeight(item.row.actualWeight, item.row.purity) }}g</span>
                    </template>
                  </el-table-column>
                  <el-table-column :label="$t('admin.settlement.table.ratio')" width="80" align="center" prop="settlementRatio" :excel-formatter="(row) => row.settlementRatio + '%'">
                    <template #default="item">
                      <span>{{ item.row.settlementRatio }}%</span>
                    </template>
                  </el-table-column>
                  <el-table-column :label="$t('admin.settlement.table.amount')" width="120" align="right" prop="settlementAmount" :excel-formatter="(row) => '₩ ' + formatPrice(row.settlementAmount || row.totalPrice || row.price || (((row.retailerConfirmMaterialCost || row.factoryInputMaterialCost || 0) + (row.retailerConfirmLaborCost || row.factoryInputLaborCost || 0)) * (row.quantity || 1)))">
                    <template #default="item">
                      <span style="font-weight: bold; color: #f56c6c;">₩ {{ formatPrice(item.row.settlementAmount || item.row.totalPrice || item.row.price || (((item.row.retailerConfirmMaterialCost || item.row.factoryInputMaterialCost || 0) + (item.row.retailerConfirmLaborCost || item.row.factoryInputLaborCost || 0)) * (item.row.quantity || 1))) }}</span>
                    </template>
                  </el-table-column>
                  <el-table-column :label="$t('admin.settlement.table.memo')" min-width="150" prop="settlementMemo" />
                </base-table>
              </div>
            </template>
          </el-table-column>

          <el-table-column :label="$t('admin.settlement.table.settledDate')" width="160" align="center" prop="updatedAt" :excel-formatter="(row) => formatDate(row.updatedAt || row.createdAt)">
            <template #default="{row}">
              <span>{{ formatDate(row.updatedAt || row.createdAt) }}</span>
            </template>
          </el-table-column>
          <el-table-column :label="$t('order.filters.orderNo')" prop="orderNo" width="200" align="center" />
          <el-table-column :label="$t('orderDetail.headers.productInfo')" min-width="220" :excel-formatter="productSummaryFormatter">
            <template #default="{row}">
              <div v-if="primaryOrderItem(row)" class="product-info-cell">
                <el-image :src="primaryOrderItem(row).photoUrl || defaultImage" fit="cover" class="product-thumb" style="width: 40px; height: 40px;" />
                <div class="product-text">
                  <div class="product-name">
                    {{ primaryOrderItem(row).productName || primaryOrderItem(row).productSetTitle }}
                    <el-tag v-if="orderItemCount(row) > 1" size="small" type="info" effect="plain" style="margin-left: 0.3125rem;">+{{ orderItemCount(row) - 1 }}</el-tag>
                  </div>
                  <div class="product-no">{{ primaryOrderItem(row).productNo }}</div>
                </div>
              </div>
              <span v-else>-</span>
            </template>
          </el-table-column>
          <el-table-column :label="$t('admin.settlement.filters.company')" width="180" align="center" prop="userDisplayName" :excel-formatter="(row) => `${row.userDisplayName} (${row.userName})`">
            <template #default="{row}">
              <span>{{ row.userDisplayName }} ({{ row.userName }})</span>
            </template>
          </el-table-column>
          <el-table-column :label="$t('admin.settlement.summary.totalAmount')" width="150" align="right" prop="totalSettlementAmount" :excel-formatter="(row) => '₩ ' + formatPrice(calculateOrderTotalSettlement(row))">
            <template #default="{row}">
              <span style="font-weight: bold; color: #f56c6c;">₩ {{ formatPrice(calculateOrderTotalSettlement(row)) }}</span>
            </template>
          </el-table-column>
          <el-table-column :label="$t('common.action')" width="180" align="center" :fixed="!isMobile ? 'right' : false">
            <template #default="{row}">
              <el-button type="primary" size="small" :icon="Printer" @click="printStatement(row)">
                {{ $t('common.action') === '작업' ? '거래명세서' : $t('common.action') }}
              </el-button>
            </template>
          </el-table-column>
        </base-table>

        <div class="pagination-container">
          <el-pagination
            v-model:current-page="listQuery.page"
            v-model:page-size="listQuery.pageSize"
            :total="total"
            :page-sizes="[10, 20, 30, 50]"
            layout="total, sizes, prev, pager, next, jumper"
            @size-change="getList"
            @current-change="getList"
          />
        </div>
      </el-card>
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
          <el-table-column label="" width="70" align="center">
            <template #default="{row}">
              <el-button size="small" text type="primary" @click="openLedgerDetail(row)">상세</el-button>
            </template>
          </el-table-column>
          <el-table-column label="거래번호" width="100" align="center" prop="id" />
          <el-table-column label="거래처" width="180" align="center">
            <template #default="{row}">
              {{ row.logisticsCompanyName }}
            </template>
          </el-table-column>
          <el-table-column label="거래일자" width="160" align="center">
            <template #default="{row}">
              {{ formatDate(row.createdAt) }}
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
                <el-button v-if="!row.isCancelled && row.isMostRecentPayment" size="small" type="warning" @click="openEditDialog(row)">수정</el-button>
                <el-button v-if="row.isCancelled" size="small" type="danger" @click="handleDeletePayable(row)">삭제</el-button>
              </div>
            </template>
          </el-table-column>
        </base-table>
      </el-card>

      <payable-edit-dialog
        v-model="editDialogVisible"
        :record="editingRecord"
        :company="editingCompany"
        @saved="onEditSaved"
      />

      <payment-application-edit-dialog
        v-model="applicationEditDialogVisible"
        :record="editingApplication"
        :company-name="editingApplicationCompanyName"
        @saved="onApplicationEditSaved"
      />

      <base-popup v-model="ledgerDetailVisible" title="정산 상세" width="900px">
        <div v-if="ledgerDetailRow" class="order-detail-expand" v-loading="paymentApplicationsLoading[ledgerDetailRow.id]">
          <el-alert v-if="ledgerDetailRow.isCancelled" type="info" :closable="false" show-icon>
            이 정산은 취소되어 적용된 제품 내역이 없습니다. 취소 시 연결된 청구 금액이 원래대로 복구되었습니다.
          </el-alert>
          <template v-else>
          <h4>적용 제품 내역</h4>
          <base-table :data="paymentApplicationsData[ledgerDetailRow.id] || []" border size="small" style="width: 100%" row-key="chargeId">
            <el-table-column label="주문번호" width="180" align="center">
              <template #default="{row: item}">
                <span class="order-link" @click="goToOrder(item.orderNo)">{{ item.orderNo }}</span>
              </template>
            </el-table-column>
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
            <el-table-column label="적용 금액" width="140" align="right">
              <template #default="{row: item}">
                <span style="color: #67c23a; font-weight: bold;">₩ {{ formatPrice(item.appliedAmount) }}</span>
              </template>
            </el-table-column>
            <el-table-column label="적용 중량" width="120" align="right">
              <template #default="{row: item}">
                {{ getEffectiveAppliedWeight(item).toFixed(2) }}g
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
                <td class="ledger-readonly">{{ getLedgerForRow(ledgerDetailRow).beforeWeight.toFixed(2) }}</td>
                <td class="ledger-readonly">₩ {{ formatPrice(getLedgerForRow(ledgerDetailRow).beforeAmount) }}</td>
              </tr>
              <tr>
                <td class="ledger-label">청구(B)</td>
                <td class="ledger-readonly">0.00</td>
                <td class="ledger-readonly">₩ 0</td>
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

    <div id="print-area" v-if="printData" style="display: none;">
      <div class="receipt-double-container">
        <template v-for="copyType in ['고객용', '보관용']" :key="copyType">
          <div class="receipt-copy-block">
            <!-- Header Table -->
            <table class="receipt-header-table">
              <tr>
                <td class="header-title-cell">
                  <span class="company-tag">[{{ isMfg ? (printData.companyName || '제조사') : (printData.userDisplayName || printData.userName || printData.companyName || '거래처') }}]</span>
                  <span class="main-title">임가공 거래 명세서</span>
                  <span class="copy-badge" :class="{ 'blue-badge': copyType === '고객용' }">({{ copyType }})</span>
                </td>
                <td class="supplier-info-cell" rowspan="2">
                  <div>공급자: {{ isMfg ? (printData.companyName || '제조사') : (printData.logisticsCompanyName || '골든바') }}</div>
                  <div>전 화: {{ printData.logisticsCompanyPhone || printData.userPhone || '051-633-1116' }}</div>
                  <div>팩 스: {{ printData.logisticsCompanyFax || '-' }}</div>
                </td>
              </tr>
              <tr>
                <td class="header-meta-cell">
                  <span>일자: {{ formatDate(printData.createdAt) }}</span>
                  <span class="meta-separator">|</span>
                  <span>거래No: {{ printData.id || printData.orderNo }}</span>
                </td>
              </tr>
            </table>

            <!-- Order Items Table -->
            <table class="receipt-items-table">
              <thead>
                <tr>
                  <th width="12%">No</th>
                  <th width="42%">주문내용</th>
                  <th width="12%">함량</th>
                  <th width="13%">실중량</th>
                  <th width="10%">주문수량</th>
                  <th width="11%">공임비</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in paddedOrderItems(printData.orderItems, 6)" :key="item.id">
                  <template v-if="!item.isDummy">
                    <td align="center">{{ item.productNo || item.id }}</td>
                    <td>
                      <div class="item-info-flex">
                        <img v-if="item.photoUrl" :src="item.photoUrl" class="item-img" />
                        <span class="item-name">{{ item.productName || item.productSetTitle }}</span>
                      </div>
                    </td>
                    <td align="center">{{ codeMap[item.purity] || item.purity || '-' }}</td>
                    <td align="right">{{ (item.actualWeight || item.requestedWeight || 0).toFixed(3) }}g</td>
                    <td align="center">{{ item.quantity }}개</td>
                    <td align="right">{{ formatPrice(isMfg ? ((item.factoryInputMaterialCost || 0) + (item.factoryInputLaborCost || 0)) * (item.quantity || 1) : (item.settlementAmount || item.totalPrice || 0)) }}</td>
                  </template>
                  <template v-else>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                  </template>
                </tr>
              </tbody>
            </table>

            <!-- Purity Summary Table -->
            <table class="receipt-purity-table">
              <thead>
                <tr>
                  <th width="33%">14K 합계(g)</th>
                  <th width="33%">18K 합계(g)</th>
                  <th width="34%">순금(24K) 합계(g)</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td align="right">{{ getPurityTotals(printData.orderItems).p14.toFixed(2) }}g</td>
                  <td align="right">{{ getPurityTotals(printData.orderItems).p18.toFixed(2) }}g</td>
                  <td align="right">{{ getPurityTotals(printData.orderItems).pure.toFixed(2) }}g</td>
                </tr>
              </tbody>
            </table>

            <!-- Settlement Balance Table -->
            <table class="receipt-balance-table">
              <thead>
                <tr>
                  <th width="30%"></th>
                  <th width="22%">순금(g)</th>
                  <th width="24%">공임 및 현금</th>
                  <th width="24%">금액 합계</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td class="row-label">최근 결제</td>
                  <td></td>
                  <td></td>
                  <td align="center" style="font-size: 9px;">{{ formatDate(printData.createdAt) }}</td>
                </tr>
                <tr>
                  <td class="row-label">거래 전 미수(A)</td>
                  <td align="right">{{ (printData.beforeWeight || 0).toFixed(2) }}</td>
                  <td align="right">{{ formatPrice(printData.beforeAmount || 0) }}</td>
                  <td></td>
                </tr>
                <tr>
                  <td class="row-label">판매(B)</td>
                  <td align="right">{{ (calculateOrderTotalPureWeight(printData) || 0).toFixed(2) }}</td>
                  <td align="right">{{ formatPrice(calculateOrderTotalSettlement(printData) || 0) }}</td>
                  <td></td>
                </tr>
                <tr>
                  <td class="row-label">결제(C)</td>
                  <td align="right">{{ (printData.paidWeight || 0).toFixed(2) }}</td>
                  <td align="right">{{ formatPrice(printData.paidAmount || 0) }}</td>
                  <td></td>
                </tr>
                <tr>
                  <td class="row-label">할인(D)</td>
                  <td align="right">0.00</td>
                  <td align="right">{{ formatPrice(printData.discountAmount || 0) }}</td>
                  <td></td>
                </tr>
                <tr class="after-balance-row">
                  <td class="row-label"><strong>거래 후 미수<br/>(A+B-C-D)</strong></td>
                  <td align="right"><strong>{{ ((printData.beforeWeight || 0) + (calculateOrderTotalPureWeight(printData) || 0) - (printData.paidWeight || 0)).toFixed(2) }}</strong></td>
                  <td align="right"><strong>{{ formatPrice((printData.beforeAmount || 0) + (calculateOrderTotalSettlement(printData) || 0) - (printData.paidAmount || 0) - (printData.discountAmount || 0)) }}</strong></td>
                  <td></td>
                </tr>
              </tbody>
            </table>
          </div>
        </template>
      </div>
    </div>

    <div id="print-area-combined" v-if="combinedPrintData" style="display: none;">
      <div class="receipt-double-container">
        <template v-for="copyType in ['고객용', '보관용']" :key="copyType">
          <div class="receipt-copy-block">
            <!-- Header Table -->
            <table class="receipt-header-table">
              <tr>
                <td class="header-title-cell">
                  <span class="company-tag">[{{ combinedPrintData.partnerTitle }}]</span>
                  <span class="main-title">일괄 거래 명세서</span>
                  <span class="copy-badge" :class="{ 'blue-badge': copyType === '고객용' }">({{ copyType }})</span>
                </td>
                <td class="supplier-info-cell" rowspan="2">
                  <div>공급자: {{ isMfg ? '제조사' : '골든바' }}</div>
                  <div>전 화: 051-633-1116</div>
                  <div>총 거래: {{ combinedPrintData.orderCount }}건</div>
                </td>
              </tr>
              <tr>
                <td class="header-meta-cell">
                  <span>기간/일자: {{ combinedPrintData.dateTitle }}</span>
                  <span class="meta-separator">|</span>
                  <span>출력일시: {{ parseTime(new Date(), '{y}-{m}-{d} {h}:{i}') }}</span>
                </td>
              </tr>
            </table>

            <!-- Order Items Table -->
            <table class="receipt-items-table">
              <thead>
                <tr>
                  <th width="12%">No</th>
                  <th width="42%">주문내용</th>
                  <th width="12%">함량</th>
                  <th width="13%">실중량</th>
                  <th width="10%">주문수량</th>
                  <th width="11%">공임/금액</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(item, idx) in combinedPrintData.items" :key="idx">
                  <template v-if="!item.isDummy">
                    <td align="center">{{ item.productNo || (idx + 1) }}</td>
                    <td>
                      <div class="item-info-flex">
                        <img v-if="item.photoUrl" :src="item.photoUrl" class="item-img" />
                        <span class="item-name">{{ item.productName || item.productSetTitle }}</span>
                      </div>
                    </td>
                    <td align="center">{{ codeMap[item.purity] || item.purity || '-' }}</td>
                    <td align="right">{{ (item.actualWeight || item.requestedWeight || 0).toFixed(3) }}g</td>
                    <td align="center">{{ item.quantity || 1 }}개</td>
                    <td align="right">{{ formatPrice(item.settlementPrice || 0) }}</td>
                  </template>
                  <template v-else>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                    <td>&nbsp;</td>
                  </template>
                </tr>
              </tbody>
            </table>

            <!-- Purity Summary Table -->
            <table class="receipt-purity-table">
              <thead>
                <tr>
                  <th width="33%">14K 합계(g)</th>
                  <th width="33%">18K 합계(g)</th>
                  <th width="34%">순금(24K) 합계(g)</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td align="right">{{ getPurityTotals(combinedPrintData.items).p14.toFixed(2) }}g</td>
                  <td align="right">{{ getPurityTotals(combinedPrintData.items).p18.toFixed(2) }}g</td>
                  <td align="right">{{ getPurityTotals(combinedPrintData.items).pure.toFixed(2) }}g</td>
                </tr>
              </tbody>
            </table>

            <!-- Settlement Balance Table -->
            <table class="receipt-balance-table">
              <thead>
                <tr>
                  <th width="30%"></th>
                  <th width="22%">순금(g)</th>
                  <th width="24%">공임 및 현금</th>
                  <th width="24%">금액 합계</th>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td class="row-label">최근 결제</td>
                  <td></td>
                  <td></td>
                  <td align="center" style="font-size: 9px;">{{ combinedPrintData.lastPaymentDate || '-' }}</td>
                </tr>
                <tr>
                  <td class="row-label">거래 전 미수(A)</td>
                  <td align="right">{{ (combinedPrintData.beforeWeight || 0).toFixed(2) }}</td>
                  <td align="right">{{ formatPrice(combinedPrintData.beforeAmount || 0) }}</td>
                  <td></td>
                </tr>
                <tr>
                  <td class="row-label">판매(B)</td>
                  <td align="right">{{ (combinedPrintData.totalFineGold || 0).toFixed(2) }}</td>
                  <td align="right">{{ formatPrice(combinedPrintData.totalSettlementAmount || 0) }}</td>
                  <td></td>
                </tr>
                <tr>
                  <td class="row-label">결제(C)</td>
                  <td align="right">{{ (combinedPrintData.paidWeight || 0).toFixed(2) }}</td>
                  <td align="right">{{ formatPrice(combinedPrintData.paidAmount || 0) }}</td>
                  <td></td>
                </tr>
                <tr>
                  <td class="row-label">할인(D)</td>
                  <td align="right">0.00</td>
                  <td align="right">{{ formatPrice(combinedPrintData.discountAmount || 0) }}</td>
                  <td></td>
                </tr>
                <tr class="after-balance-row">
                  <td class="row-label"><strong>거래 후 미수<br/>(A+B-C-D)</strong></td>
                  <td align="right"><strong>{{ ((combinedPrintData.beforeWeight || 0) + (combinedPrintData.totalFineGold || 0) - (combinedPrintData.paidWeight || 0)).toFixed(2) }}</strong></td>
                  <td align="right"><strong>{{ formatPrice((combinedPrintData.beforeAmount || 0) + (combinedPrintData.totalSettlementAmount || 0) - (combinedPrintData.paidAmount || 0) - (combinedPrintData.discountAmount || 0)) }}</strong></td>
                  <td></td>
                </tr>
              </tbody>
            </table>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ElMessage, ElMessageBox } from 'element-plus';
import { useMobile } from '@/hooks/useMobile';
import { ref, reactive, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { getAllOrders, getSettlementSummary } from '@/api/order';
import { getPayables, getPaymentApplications, getCompanySummaries, updatePayable, deletePayable, getLedgerBefore } from '@/api/payable';
import { Printer } from '@element-plus/icons-vue';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import useCodeStore from '@/store/modules/code';
import useUserStore from '@/store/modules/user';
import BaseTable from '@/components/BaseTable/index.vue';
import BasePopup from '@/components/BasePopup/index.vue';
import SettlementHistoryFilter from './components/SettlementHistoryFilter.vue';
import PayableEditDialog from './components/PayableEditDialog.vue';
import PaymentApplicationEditDialog from './components/PaymentApplicationEditDialog.vue';

const { isMobile } = useMobile();
const router = useRouter();
const userStore = useUserStore();
const isMfg = computed(() => userStore.companyType === 'MFG');
// DCC's own settled-order history now lives in the retail-style branch below (scoped to
// their own orders via the backend's automatic LogisticsCompanyId filter for DCC users),
// same as RTL/admin - only MFG still needs the separate payment-transaction ledger.
const isPayableSide = computed(() => userStore.companyType === 'MFG');

const listLoading = ref(true);
const summaryLoading = ref(true);
const list = ref<any[]>([]);
const total = ref(0);
const codeStore = useCodeStore();
const codeMap = computed(() => codeStore.codeMap);
const defaultImage = '/thumb_no_img.png';

const summaryData = reactive({
  totalActualWeight: 0,
  totalFineGold: 0,
  totalSettlementAmount: 0
});

const end = new Date();
const start = new Date();
start.setTime(start.getTime() - 3600 * 1000 * 24 * 30);
const defaultStartDate = parseTime(start, '{y}-{m}-{d}');
const defaultEndDate = parseTime(end, '{y}-{m}-{d}');

const listQuery = reactive({
  page: 1,
  pageSize: 20,
  status: 'SETTLED',
  orderNo: '',
  userName: '',
  companyId: undefined as number | undefined,
  startDate: defaultStartDate,
  endDate: defaultEndDate,
  categoryLarge: '',
  categoryMedium: '',
  categorySmall: '',
  setCategoryLarge: '',
  setCategoryMedium: '',
  setCategorySmall: ''
});

const printData = ref<any>(null);

const formatDate = (dateStr: string) => {
  if (!dateStr) return '';
  return parseTime(new Date(dateStr), '{y}-{m}-{d} {h}:{i}');
};

const calculatePurityWeight = (weight: number, purity: string) => {
  if (!weight) return '0.00';
  let ratio = 0;
  switch (purity) {
    case '14K': ratio = 0.6435; break;
    case '18K': ratio = 0.825; break;
    case '24K': ratio = 1.0; break;
    case 'PT': ratio = 0.95; break;
    default: ratio = 0;
  }
  return (weight * ratio).toFixed(2);
};

const primaryOrderItem = (row: any) => {
  return row.orderItems && row.orderItems.length > 0 ? row.orderItems[0] : null;
};

const orderItemCount = (row: any) => {
  return row.orderItems ? row.orderItems.length : 0;
};

const productSummaryFormatter = (row: any) => {
  const item = primaryOrderItem(row);
  if (!item) return '-';
  const name = item.productName || item.productSetTitle || '';
  const extra = orderItemCount(row) > 1 ? ` 외 ${orderItemCount(row) - 1}건` : '';
  return `${name}${extra}`;
};

const flattenOrderItems = (orderItems: any[]) => {
  const result: any[] = [];
  if (!orderItems) return result;
  orderItems.forEach(item => {
    result.push({ ...item, depth: 0 });
    if (item.children && item.children.length > 0) {
      item.children.forEach((child: any) => {
        result.push({ ...child, depth: 1 });
      });
    }
  });
  return result;
};

const paddedOrderItems = (orderItems: any[], minCount: number = 6) => {
  const flattened = flattenOrderItems(orderItems || []);
  const result = [...flattened];
  while (result.length < minCount) {
    result.push({ isDummy: true, id: `dummy-${result.length}` });
  }
  return result;
};

// 14K/18K raw totals plus the true fine-gold (24K-converted) total across every purity,
// for the retail-side receipt's own print templates (#print-area/#print-area-combined) -
// same computation as buildStatementPurityRows on the payable side's receipt.
const getPurityTotals = (items: any[]) => {
  const flat = flattenOrderItems(items || []).filter((item: any) => !item.isDummy);
  let p14 = 0;
  let p18 = 0;
  let pure = 0;
  flat.forEach((item: any) => {
    const weight = (item.actualWeight || item.requestedWeight || 0) * (item.quantity || 1);
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

const calculateOrderTotalSettlement = (order: any) => {
  if (!order) return 0;
  if (isMfg.value) {
    let mfgTotal = 0;
    if (order.orderItems) {
      order.orderItems.forEach((item: any) => {
        mfgTotal += ((item.factoryInputMaterialCost || 0) + (item.factoryInputLaborCost || 0)) * (item.quantity || 1);
        if (item.children) {
          item.children.forEach((c: any) => {
            mfgTotal += ((c.factoryInputMaterialCost || 0) + (c.factoryInputLaborCost || 0)) * (c.quantity || 1);
          });
        }
      });
    }
    return mfgTotal;
  }

  let total = 0;
  if (order.orderItems && order.orderItems.length > 0) {
    order.orderItems.forEach((item: any) => {
      const itemPrice = item.settlementAmount || item.totalPrice || item.price || item.laborCost ||
        (((item.retailerConfirmMaterialCost || item.factoryInputMaterialCost || 0) + (item.retailerConfirmLaborCost || item.factoryInputLaborCost || 0)) * (item.quantity || 1));
      total += itemPrice;
      if (item.children) {
        item.children.forEach((c: any) => {
          const childPrice = c.settlementAmount || c.totalPrice || c.price || c.laborCost ||
            (((c.retailerConfirmMaterialCost || c.factoryInputMaterialCost || 0) + (c.retailerConfirmLaborCost || c.factoryInputLaborCost || 0)) * (c.quantity || 1));
          total += childPrice;
        });
      }
    });
  }

  if (total > 0) {
    return total;
  }

  if (order.settlementAmount && order.settlementAmount > 0) {
    return order.settlementAmount;
  }

  return order.totalAmount || 0;
};

const calculateOrderTotalPureWeight = (order: any) => {
  if (!order || !order.orderItems) return 0;
  let total = 0;
  order.orderItems.forEach((item: any) => {
    const weight = item.actualWeight || item.requestedWeight || item.confirmedWeight || 0;
    let ratio = 0;
    switch (item.purity) {
      case '14K': ratio = 0.6435; break;
      case '18K': ratio = 0.825; break;
      case '24K': ratio = 1.0; break;
      case 'PT': ratio = 0.95; break;
      default: ratio = 0;
    }
    total += weight * ratio * (item.quantity || 1);
    if (item.children) {
      item.children.forEach((c: any) => {
        const cWeight = c.actualWeight || c.requestedWeight || c.confirmedWeight || 0;
        let cRatio = 0;
        switch (c.purity) {
          case '14K': cRatio = 0.6435; break;
          case '18K': cRatio = 0.825; break;
          case '24K': cRatio = 1.0; break;
          case 'PT': cRatio = 0.95; break;
          default: cRatio = 0;
        }
        total += cWeight * cRatio * (c.quantity || 1);
      });
    }
  });
  return total;
};

const fetchSummary = async () => {
  summaryLoading.value = true;
  try {
    const res = await getSettlementSummary(listQuery);
    Object.assign(summaryData, res.data);
  } catch (error) {
    console.error('Failed to fetch summary:', error);
  } finally {
    summaryLoading.value = false;
  }
};

const getList = async () => {
  listLoading.value = true;
  try {
    const [res] = await Promise.all([
      getAllOrders(listQuery),
      codeStore.fetchCodes()
    ]);
    list.value = res.data.items;
    total.value = res.data.totalCount;
    fetchSummary();
  } catch (error) {
    console.error('Failed to get orders:', error);
  } finally {
    listLoading.value = false;
  }
};

const handleFilter = () => {
  listQuery.page = 1;
  getList();
};

const resetQuery = () => {
  listQuery.orderNo = '';
  listQuery.userName = '';
  listQuery.companyId = undefined;
  listQuery.startDate = undefined;
  listQuery.endDate = undefined;
  listQuery.categoryLarge = '';
  listQuery.categoryMedium = '';
  listQuery.categorySmall = '';
  listQuery.setCategoryLarge = '';
  listQuery.setCategoryMedium = '';
  listQuery.setCategorySmall = '';
  handleFilter();
};

const printStatement = (row: any) => {
  printData.value = row;
  setTimeout(() => {
    const printContents = document.getElementById('print-area')?.innerHTML;
    if (!printContents) return;

    const printWindow = window.open('', '_blank');
    if (printWindow) {
      printWindow.document.write(`
        <html>
        <head>
          <title>거래명세서 - ${row.orderNo || ''}</title>
          <style>
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
          </style>
        </head>
        <body>
      `);
      printWindow.document.write(printContents);
      printWindow.document.write('<script>window.onload = () => { window.print(); setTimeout(() => window.close(), 500); };<\/script>');
      printWindow.document.write('</body></html>');
      printWindow.document.close();
    }
  }, 100);
};

const combinedPrintData = ref<any>(null);

const printCombinedStatement = () => {
  if (!list.value || list.value.length === 0) {
    ElMessage.warning('출력할 거래 내역이 없습니다.');
    return;
  }

  let dateTitle = '전체 기간';
  if (listQuery.startDate && listQuery.endDate) {
    if (listQuery.startDate === listQuery.endDate) {
      dateTitle = listQuery.startDate;
    } else {
      dateTitle = `${listQuery.startDate} ~ ${listQuery.endDate}`;
    }
  } else if (listQuery.startDate) {
    dateTitle = `${listQuery.startDate} ~`;
  } else if (listQuery.endDate) {
    dateTitle = `~ ${listQuery.endDate}`;
  }

  let partnerTitle = '전체 거래처';
  if (listQuery.companyId) {
    const firstOrder = list.value[0];
    partnerTitle = firstOrder?.companyName || firstOrder?.userDisplayName || firstOrder?.userName || '선택 거래처';
  }

  let totalActualWeight = 0;
  let totalFineGold = 0;
  let totalSettlementAmount = 0;
  let beforeWeight = list.value[0]?.beforeWeight || 0;
  let beforeAmount = list.value[0]?.beforeAmount || 0;
  let lastPaymentDate = list.value[0]?.createdAt ? formatDate(list.value[0].createdAt) : '-';
  let paidWeight = 0;
  let paidAmount = 0;
  let discountAmount = 0;
  const items: any[] = [];

  list.value.forEach((order: any) => {
    paidWeight += (order.paidWeight || 0);
    paidAmount += (order.paidAmount || 0);
    discountAmount += (order.discountAmount || 0);
    totalSettlementAmount += calculateOrderTotalSettlement(order);
    if (order.orderItems) {
      const flattened = flattenOrderItems(order.orderItems);
      flattened.forEach((item: any) => {
        const weight = item.actualWeight || item.requestedWeight || 0;
        const fineStr = calculatePurityWeight(weight, item.purity);
        totalActualWeight += weight;
        totalFineGold += parseFloat(fineStr || '0');

        const itemPrice = item.settlementAmount || item.totalPrice || item.price || item.laborCost ||
          (((item.retailerConfirmMaterialCost || item.factoryInputMaterialCost || 0) + (item.retailerConfirmLaborCost || item.factoryInputLaborCost || 0)) * (item.quantity || 1));

        items.push({
          ...item,
          settlementPrice: itemPrice
        });
      });
    }
  });

  const paddedItems = paddedOrderItems(items, 6);

  combinedPrintData.value = {
    dateTitle,
    partnerTitle,
    orderCount: list.value.length,
    items: paddedItems,
    totalActualWeight,
    totalFineGold,
    totalSettlementAmount,
    beforeWeight,
    beforeAmount,
    lastPaymentDate,
    paidWeight,
    paidAmount,
    discountAmount
  };

  setTimeout(() => {
    const printContents = document.getElementById('print-area-combined')?.innerHTML;
    if (!printContents) return;

    const printWindow = window.open('', '_blank');
    if (printWindow) {
      printWindow.document.write(`
        <html>
        <head>
          <title>일괄 거래명세서 - ${dateTitle}</title>
          <style>
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
          </style>
        </head>
        <body>
      `);
      printWindow.document.write(printContents);
      printWindow.document.write('<script>window.onload = () => { window.print(); setTimeout(() => window.close(), 500); };<\/script>');
      printWindow.document.write('</body></html>');
      printWindow.document.close();
    }
  }, 100);
};

const productItemInfoFormatter = (row: any) => {
  const purity = row.purity ? `[${codeMap.value[row.purity] || row.purity}]` : '';
  const color = row.color ? `[${codeMap.value[row.color] || row.color}]` : '';
  return `${row.productName || row.productSetTitle}\n${row.productNo}\n${purity} ${color}`;
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

const handlePayableFilter = () => {
  payableQuery.page = 1;
  fetchPayableList();
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

const openLedgerDetail = (row: any) => {
  ledgerDetailRow.value = row;
  ledgerDetailVisible.value = true;
  if (!row.isCancelled && !paymentApplicationsData[row.id]) {
    fetchPaymentApplications(row.id);
  }
};

const goToOrder = (orderNo: string) => {
  router.push({ path: '/order/order-tracking', query: { orderNo } });
};

// company.totalOutstanding already reflects this payment's effect (it's the CURRENT
// balance), so adding this record's own amount/discount back gives the balance as it
// was before this payment was ever applied - shared by the print statement, the inline
// expand recap, and PayableEditDialog's ledger.
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

// Amount and weight settle a charge together (either side fully paid clears both), so a
// literal 0 here can mean "this application's own weight pool never had to touch this
// charge, because the amount side already cleared it" - not "no weight was owed". When
// the charge is now fully settled but this application recorded 0 weight, show the
// charge's own weight instead so it doesn't read as unpaid.
const getEffectiveAppliedWeight = (item: any) => {
  if (item.appliedWeight > 0) return item.appliedWeight;
  if ((item.chargeRemainingWeight || 0) <= 0 && (item.chargeRemainingAmount || 0) <= 0) {
    return item.chargeWeight || 0;
  }
  return item.appliedWeight || 0;
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
  const { beforeAmount, beforeWeight } = getLedgerForRow(row);
  const form = getLedgerEditForm(row);
  return {
    afterAmount: beforeAmount - (form.amount || 0) - (form.discount || 0),
    afterWeight: beforeWeight - (form.weight || 0) - (form.discountWeight || 0)
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

const buildStatementCopy = (copyLabel: string, opts: { companyTag: string; supplierName: string; date: string; transactionNo: string | number; itemRows: string; purityRows: string; balanceRows: string }) => `
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
    <table class="receipt-items-table">
      <thead><tr><th width="12%">No</th><th width="42%">주문내용</th><th width="12%">함량</th><th width="13%">실중량</th><th width="10%">주문수량</th><th width="11%">공임비</th></tr></thead>
      <tbody>${opts.itemRows}</tbody>
    </table>
    <table class="receipt-purity-table">
      <thead><tr><th width="33%">14K 합계(g)</th><th width="33%">18K 합계(g)</th><th width="34%">순금(24K) 합계(g)</th></tr></thead>
      <tbody>${opts.purityRows}</tbody>
    </table>
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
  const allItems = (paymentApplicationsData[record.id] || []).flatMap((app: any) => app.items || []);

  const ledger = await getAccurateLedger(record.id, record);
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

  combinedList.forEach((p: any) => {
    totalAmount += p.amount || 0;
    totalWeight += p.weight || 0;
    totalDiscount += p.discount || 0;
    totalDiscountWeight += p.discountWeight || 0;
    (paymentApplicationsData[p.id] || []).forEach((app: any) => {
      allItems.push(...(app.items || []));
    });
  });

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

  const ledger = {
    company,
    beforeAmount,
    beforeWeight,
    newChargeAmount,
    newChargeWeight,
    afterAmount: beforeAmount + newChargeAmount - totalAmount - totalDiscount,
    afterWeight: beforeWeight + newChargeWeight - totalWeight - totalDiscountWeight
  };

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
  const ledger = await getAccurateLedger(record.id, record);
  const supplierName = !isMfg.value ? ledger.company.companyName : userStore.companyName || '-';
  const payerName = !isMfg.value ? userStore.companyName || '-' : ledger.company.companyName;

  const itemRows = buildStatementItemRows(item.items || []);
  const purityRows = buildStatementPurityRows(item.items || []);
  const balanceRows = buildStatementBalanceRows(ledger, record);

  const bodyHtml = ['고객용', '보관용'].map((label) => buildStatementCopy(label, {
    companyTag: payerName,
    supplierName,
    date: formatDate(record.createdAt),
    transactionNo: item.chargeId,
    itemRows,
    purityRows,
    balanceRows
  })).join('');

  openStatementPrintWindow(`정산 영수증 - ${item.orderNo}`, bodyHtml);
};

const editDialogVisible = ref(false);
const editingRecord = ref<any>(null);
const editingCompany = ref<any>(null);

const openEditDialog = (record: any) => {
  editingRecord.value = record;
  editingCompany.value = getCompanyForRow(record);
  editDialogVisible.value = true;
};

const onEditSaved = () => {
  fetchPayableList();
  fetchCompanySummaries();
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

onMounted(() => {
  if (isPayableSide.value) {
    fetchCompanySummaries();
    fetchPayableList();
  } else {
    getList();
  }
});
</script>

<style lang="scss" scoped>
@import "./SettlementHistoryStyles.scss";
</style>

