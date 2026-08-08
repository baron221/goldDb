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
          <el-table-column :label="$t('admin.settlement.table.receivableLink')" width="150" align="center">
            <template #default="{row}">
              <el-button link size="small" @click="goToReceivable(row.userId)">{{ $t('admin.settlement.table.receivableCheck') }}</el-button>
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

      <el-card shadow="never" style="margin-top: 1.25rem;">
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
          @expand-change="handlePayableExpandChange"
        >
          <el-table-column type="expand">
            <template #default="{row}">
              <div class="order-detail-expand" v-loading="paymentApplicationsLoading[row.id]">
                <h4>적용 제품 내역</h4>
                <base-table :data="paymentApplicationsData[row.id] || []" border size="small" style="width: 100%" row-key="chargeId">
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
                      {{ item.appliedWeight.toFixed(2) }}g
                    </template>
                  </el-table-column>
                  <el-table-column label="작업" width="180" align="center">
                    <template #default>
                      <div style="display: flex; gap: 0.375rem; justify-content: center;">
                        <el-button size="small" @click="handleIssueStatement(row)">거래명세서</el-button>
                        <el-button v-if="!row.isCancelled" size="small" type="warning" @click="openEditDialog(row)">수정</el-button>
                      </div>
                    </template>
                  </el-table-column>
                </base-table>

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
                      <td class="ledger-readonly">{{ getLedgerForRow(row).beforeWeight.toFixed(2) }}</td>
                      <td class="ledger-readonly">₩ {{ formatPrice(getLedgerForRow(row).beforeAmount) }}</td>
                    </tr>
                    <tr>
                      <td class="ledger-label">청구(B)</td>
                      <td class="ledger-readonly">0.00</td>
                      <td class="ledger-readonly">₩ 0</td>
                    </tr>
                    <tr v-if="!row.isCancelled">
                      <td class="ledger-label">결제(C)</td>
                      <td>
                        <el-input-number v-model="getLedgerEditForm(row).weight" :min="0" :precision="2" :step="0.1" size="small" style="width: 100%;" />
                      </td>
                      <td>
                        <el-input-number v-model="getLedgerEditForm(row).amount" :min="0" :step="1000" size="small" style="width: 100%;" />
                      </td>
                    </tr>
                    <tr v-if="!row.isCancelled">
                      <td class="ledger-label">할인(D)</td>
                      <td>
                        <el-input-number v-model="getLedgerEditForm(row).discountWeight" :min="0" :precision="2" :step="0.1" size="small" style="width: 100%;" />
                      </td>
                      <td>
                        <el-input-number v-model="getLedgerEditForm(row).discount" :min="0" :step="1000" size="small" style="width: 100%;" />
                      </td>
                    </tr>
                    <tr v-else>
                      <td class="ledger-label">결제(C) / 할인(D)</td>
                      <td class="ledger-readonly" colspan="2">취소된 거래는 수정할 수 없습니다.</td>
                    </tr>
                    <tr class="ledger-total-row">
                      <td class="ledger-label">거래 후 미지급(A+B-C-D)</td>
                      <td class="ledger-readonly">{{ getLedgerAfter(row).afterWeight.toFixed(2) }}</td>
                      <td class="ledger-readonly">₩ {{ formatPrice(getLedgerAfter(row).afterAmount) }}</td>
                    </tr>
                  </tbody>
                </table>
                <div v-if="!row.isCancelled" style="display: flex; justify-content: flex-end; margin-top: 0.625rem;">
                  <el-button type="primary" size="small" :loading="ledgerSaving[row.id]" @click="saveLedgerEdit(row)">저장</el-button>
                </div>
              </div>
            </template>
          </el-table-column>
          <el-table-column label="거래번호" width="100" align="center" prop="id" />
          <el-table-column label="발행처" width="180" align="center">
            <template #default="{row}">
              {{ row.manufacturerCompanyName }}
            </template>
          </el-table-column>
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
              <el-tag v-else type="success" size="small">완료</el-tag>
            </template>
          </el-table-column>
          <el-table-column label="메모" min-width="140" prop="memo" />
          <el-table-column label="작업" width="220" align="center" :fixed="!isMobile ? 'right' : false">
            <template #default="{row}">
              <div style="display: flex; gap: 0.375rem; justify-content: center;">
                <el-button size="small" @click="handleIssueStatement(row)">거래명세서</el-button>
                <template v-if="!row.isCancelled">
                  <el-button size="small" type="warning" @click="openEditDialog(row)">수정</el-button>
                  <el-button size="small" type="danger" @click="handleCancelPayable(row)">정산취소</el-button>
                </template>
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
import { getPayables, getPaymentApplications, getCompanySummaries, cancelPayable, updatePayable } from '@/api/payable';
import { Printer } from '@element-plus/icons-vue';
import { parseTime } from '@/utils';
import { formatPrice } from '@/utils/format';
import useCodeStore from '@/store/modules/code';
import useUserStore from '@/store/modules/user';
import BaseTable from '@/components/BaseTable/index.vue';
import SettlementHistoryFilter from './components/SettlementHistoryFilter.vue';
import PayableEditDialog from './components/PayableEditDialog.vue';

const { isMobile } = useMobile();
const router = useRouter();
const userStore = useUserStore();
const isMfg = computed(() => userStore.companyType === 'MFG');
const isPayableSide = computed(() => userStore.companyType === 'MFG' || userStore.companyType === 'DCC');

const listLoading = ref(true);
const summaryLoading = ref(true);
const list = ref<any[]>([]);
const total = ref(0);
const codeStore = useCodeStore();
const codeMap = computed(() => codeStore.codeMap);
const defaultImage = 'https://via.placeholder.com/100x100?text=No+Image';

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

const goToReceivable = (userId: number) => {
  router.push({
    path: '/정산/receivable-management',
    query: { userId }
  });
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

const handlePayableExpandChange = (row: any, expandedRows: any[]) => {
  if (expandedRows.includes(row) && !paymentApplicationsData[row.id]) {
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
const getLedgerForRow = (record: any) => {
  const company = getCompanyForRow(record);
  const afterAmount = company.totalOutstanding || 0;
  const afterWeight = company.totalOutstandingWeight || 0;
  const beforeAmount = afterAmount + (record.amount || 0) + (record.discount || 0);
  const beforeWeight = afterWeight + (record.weight || 0) + (record.discountWeight || 0);
  return { company, afterAmount, afterWeight, beforeAmount, beforeWeight };
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

// 거래명세서 - ported from payable-management.vue's handlePrintReceipt (the *working*
// A/B/C/D ledger), not this file's own printStatement (which reads fields that don't
// exist anywhere in the backend and always renders A/C/D as 0 - see plan notes).
const handleIssueStatement = (record: any) => {
  const printWindow = window.open('', '_blank');
  if (!printWindow) return;

  const { company, afterAmount, afterWeight, beforeAmount, beforeWeight } = getLedgerForRow(record);

  const supplierName = !isMfg.value ? company.companyName : userStore.companyName || '-';
  const payerName = !isMfg.value ? userStore.companyName || '-' : company.companyName;

  const ledgerRows = `
    <tr><td class="label">최근결제</td><td>${company.lastPaymentDate ? formatDate(company.lastPaymentDate) : '-'}</td><td></td><td></td></tr>
    <tr><td class="label">거래 전 미지급(A)</td><td>${beforeWeight.toFixed(2)}</td><td>${formatPrice(beforeAmount)}</td><td></td></tr>
    <tr><td class="label">청구(B)</td><td>0.00</td><td>0</td><td></td></tr>
    <tr><td class="label">결제(C)</td><td>${(record.weight || 0).toFixed(2)}</td><td>${formatPrice(record.amount || 0)}</td><td></td></tr>
    <tr><td class="label">할인(D)</td><td>0.00</td><td>${formatPrice(record.discount || 0)}</td><td></td></tr>
    <tr><td class="label"><strong>거래 후 미지급(A+B-C-D)</strong></td><td><strong>${afterWeight.toFixed(2)}</strong></td><td><strong>${formatPrice(afterAmount)}</strong></td><td></td></tr>
  `;

  const statementBlock = (copyLabel: string) => `
    <div class="statement-copy">
      <div class="statement-title">[${payerName}] 정산 명세서(${copyLabel})</div>
      <div class="statement-meta">
        <span>공급자: ${supplierName}</span>
        <span>일자: ${formatDate(record.createdAt)}</span>
        <span>거래No: ${record.id}</span>
      </div>
      <table>
        <thead><tr><th></th><th>순금(g)</th><th>공임 및 현금</th><th>금액 합계</th></tr></thead>
        <tbody>${ledgerRows}</tbody>
      </table>
    </div>
  `;

  const html = `
    <html>
      <head>
        <title>정산 명세서 - ${payerName}</title>
        <style>
          body { font-family: 'Malgun Gothic', sans-serif; padding: 10mm; }
          .statements-row { display: flex; gap: 10mm; }
          .statement-copy { flex: 1; min-width: 0; }
          .statement-title { font-weight: bold; font-size: 1rem; margin-bottom: 8px; }
          .statement-meta { display: flex; justify-content: space-between; font-size: 0.85rem; color: #333; margin-bottom: 8px; }
          table { width: 100%; border-collapse: collapse; }
          th, td { border: 1px solid #333; padding: 6px; text-align: center; font-size: 0.85rem; }
          th { background: #f5f5f5; }
          td.label { text-align: left; background: #fafafa; font-weight: 600; }
          .footer-note { margin-top: 16px; text-align: center; font-size: 0.85rem; color: #333; }
        </style>
      </head>
      <body>
        <div class="statements-row">
          ${statementBlock('공급자용')}
          ${statementBlock('보관용')}
        </div>
        <p class="footer-note">상기 대여 및 영수(미수는 대여로 함)합니다. (VAT 별도)</p>
        <p style="margin-top: 10px; font-size: 0.85rem;">메모: ${record.memo || '-'}</p>
        <script>window.onload = () => { window.print(); setTimeout(() => window.close(), 500); };<\/script>
      </body>
    </html>
  `;

  printWindow.document.write(html);
  printWindow.document.close();
};

// 일괄 거래명세서 - same A/B/C/D ledger as handleIssueStatement, but summed across every
// payment currently listed (respecting the active filter), with every applied order/product
// line appended below so the aggregate totals are backed by real line items, not just a sum.
const printCombinedPayableStatement = async () => {
  if (!payableList.value || payableList.value.length === 0) {
    ElMessage.warning('출력할 거래 내역이 없습니다.');
    return;
  }

  await Promise.all(payableList.value.map(async (p: any) => {
    if (!paymentApplicationsData[p.id]) {
      await fetchPaymentApplications(p.id);
    }
  }));

  const company = getCompanyForRow(payableList.value[0]);
  const afterAmount = company.totalOutstanding || 0;
  const afterWeight = company.totalOutstandingWeight || 0;

  let totalAmount = 0;
  let totalWeight = 0;
  let totalDiscount = 0;
  const lineItems: any[] = [];

  payableList.value.forEach((p: any) => {
    totalAmount += p.amount || 0;
    totalWeight += p.weight || 0;
    totalDiscount += p.discount || 0;
    (paymentApplicationsData[p.id] || []).forEach((app: any) => {
      lineItems.push({
        orderNo: app.orderNo,
        productLabel: (app.items || []).map((it: any) => it.productName).filter(Boolean).join(', ') || '-',
        appliedAmount: app.appliedAmount,
        appliedWeight: app.appliedWeight
      });
    });
  });

  const beforeAmount = afterAmount + totalAmount + totalDiscount;
  const beforeWeight = afterWeight + totalWeight;

  const supplierName = !isMfg.value ? company.companyName : userStore.companyName || '-';
  const payerName = !isMfg.value ? userStore.companyName || '-' : company.companyName;

  const dateTitle = payableQuery.startDate && payableQuery.endDate
    ? (payableQuery.startDate === payableQuery.endDate ? payableQuery.startDate : `${payableQuery.startDate} ~ ${payableQuery.endDate}`)
    : '전체 기간';

  const itemRows = lineItems.map((it) => `
    <tr>
      <td>${it.orderNo || '-'}</td>
      <td style="text-align: left;">${it.productLabel}</td>
      <td>₩ ${formatPrice(it.appliedAmount)}</td>
      <td>${(it.appliedWeight || 0).toFixed(2)}g</td>
    </tr>
  `).join('');

  const ledgerRows = `
    <tr><td class="label">최근결제</td><td>${company.lastPaymentDate ? formatDate(company.lastPaymentDate) : '-'}</td><td></td><td></td></tr>
    <tr><td class="label">거래 전 미지급(A)</td><td>${beforeWeight.toFixed(2)}</td><td>${formatPrice(beforeAmount)}</td><td></td></tr>
    <tr><td class="label">청구(B)</td><td>0.00</td><td>0</td><td></td></tr>
    <tr><td class="label">결제(C)</td><td>${totalWeight.toFixed(2)}</td><td>${formatPrice(totalAmount)}</td><td></td></tr>
    <tr><td class="label">할인(D)</td><td>0.00</td><td>${formatPrice(totalDiscount)}</td><td></td></tr>
    <tr><td class="label"><strong>거래 후 미지급(A+B-C-D)</strong></td><td><strong>${afterWeight.toFixed(2)}</strong></td><td><strong>${formatPrice(afterAmount)}</strong></td><td></td></tr>
  `;

  const statementBlock = (copyLabel: string) => `
    <div class="statement-copy">
      <div class="statement-title">[${payerName}] 일괄 정산 명세서(${copyLabel})</div>
      <div class="statement-meta">
        <span>공급자: ${supplierName}</span>
        <span>기간: ${dateTitle}</span>
        <span>거래 ${payableList.value.length}건</span>
      </div>
      <table class="items-table">
        <thead><tr><th>주문번호</th><th style="text-align: left;">제품정보</th><th>적용 금액</th><th>적용 중량</th></tr></thead>
        <tbody>${itemRows || '<tr><td colspan="4">적용된 제품 내역이 없습니다.</td></tr>'}</tbody>
      </table>
      <table style="margin-top: 8px;">
        <thead><tr><th></th><th>순금(g)</th><th>공임 및 현금</th><th>금액 합계</th></tr></thead>
        <tbody>${ledgerRows}</tbody>
      </table>
    </div>
  `;

  const html = `
    <html>
      <head>
        <title>일괄 정산 명세서 - ${payerName}</title>
        <style>
          body { font-family: 'Malgun Gothic', sans-serif; padding: 10mm; }
          .statements-row { display: flex; gap: 10mm; }
          .statement-copy { flex: 1; min-width: 0; }
          .statement-title { font-weight: bold; font-size: 1rem; margin-bottom: 8px; }
          .statement-meta { display: flex; justify-content: space-between; font-size: 0.85rem; color: #333; margin-bottom: 8px; }
          table { width: 100%; border-collapse: collapse; }
          th, td { border: 1px solid #333; padding: 6px; text-align: center; font-size: 0.85rem; }
          th { background: #f5f5f5; }
          td.label { text-align: left; background: #fafafa; font-weight: 600; }
          .items-table td, .items-table th { font-size: 0.8rem; }
          .footer-note { margin-top: 16px; text-align: center; font-size: 0.85rem; color: #333; }
        </style>
      </head>
      <body>
        <div class="statements-row">
          ${statementBlock('공급자용')}
          ${statementBlock('보관용')}
        </div>
        <p class="footer-note">상기 대여 및 영수(미수는 대여로 함)합니다. (VAT 별도)</p>
        <script>window.onload = () => { window.print(); setTimeout(() => window.close(), 500); };<\/script>
      </body>
    </html>
  `;

  const printWindow = window.open('', '_blank');
  if (!printWindow) return;
  printWindow.document.write(html);
  printWindow.document.close();
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

const handleCancelPayable = (record: any) => {
  ElMessageBox.confirm('이 정산 내역을 취소하시겠습니까? 관련 미지급액이 다시 복구됩니다.', '정산 취소', {
    confirmButtonText: '취소 처리',
    cancelButtonText: '닫기',
    type: 'warning'
  }).then(async () => {
    try {
      await cancelPayable(record.id);
      ElMessage.success('정산이 취소되었습니다.');
      fetchPayableList();
      fetchCompanySummaries();
    } catch (error) {
      console.error('Failed to cancel payable:', error);
      ElMessage.error('취소에 실패했습니다.');
    }
  }).catch(() => {});
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

